using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Live;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections;
using GalaSoft.MvvmLight;
using OneShoppingList.Model;
using System.Runtime.Serialization.Json;
using System.IO;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace OneShoppingList
{
    public class SkyDriveHandler: INotifyPropertyChanged
    {

        private string userName;
        public string UserName
        {
            get { return this.userName; }
            set
            {
                userName = value;
                RaisePropertyChanged("UserName");
            }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get { return this.errorMessage; }
            set
            {
                errorMessage = value;
                RaisePropertyChanged("ErrorMessage");
            }
        }

        public const string IsRunningPropertyName = "IsRunning";
        private bool isRunning = false;
        public bool IsRunning
        {
            get
            {
                return isRunning;
            }
            set
            {
                if (isRunning == value)
                {
                    return;
                }
                isRunning = value;

                this.RaisePropertyChanged(IsRunningPropertyName);

            }
        }

        private LiveOperation currentOperation;
        public LiveOperation CurrentOperation{
            get
            {
                return currentOperation;
            }
            private set
            {
                if (value == currentOperation)
                {
                    return;
                }
                currentOperation = value;
                RaisePropertyChanged("CurrentOperation");
            }
        }


        private List<string> Path = new List<string>()
        {
            "AppData", "OneFamily", "ShoppingList"
        };

        private string ApplicationFolderIDSettingsKey = "ApplicationFolderID";
        private string ApplicationFolderID
        {
            get
            {
                if (isolatedStorageSettings.Contains(ApplicationFolderIDSettingsKey))
                {
                    return isolatedStorageSettings[ApplicationFolderIDSettingsKey] as string;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (isolatedStorageSettings.Contains(ApplicationFolderIDSettingsKey))
                {
                    if (value != isolatedStorageSettings[ApplicationFolderIDSettingsKey] as string)
                    {
                        isolatedStorageSettings[ApplicationFolderIDSettingsKey] = value;
                        isolatedStorageSettings.Save();
                    }
                }
                else
                {
                    isolatedStorageSettings.Add(ApplicationFolderIDSettingsKey, value);
                    isolatedStorageSettings.Save();
                }
            }
        }

        /// <summary>
        /// Resets the internally cached application folder ID from skydrive
        /// </summary>
        public void Reset()
        {
            this.ApplicationFolderID = null;
        }

        private int currentIndex = 0;
        private string currentFolderId = null;
        private LiveAuthClient auth = null;
        private LiveConnectSession session = null;
        private LiveConnectClient client = null;
        private IList applicationFolderContent;
        private IsolatedStorageSettings isolatedStorageSettings;

        public SkyDriveHandler()
        {
            try
            {
                isolatedStorageSettings = IsolatedStorageSettings.ApplicationSettings;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: Cannot read Application Settings. ");
                Debug.WriteLine(e.ToString());
            }
        }

        public void ConnectAsync(Action<LiveOperationCompletedEventArgs> onCompleted)
        {
            if (IsRunning) return;
            auth = new LiveAuthClient(clientId: Secrets.ClientID);
            auth.InitializeCompleted += OnLoginCompleted;

            IsRunning = true;

            CurrentOperation = LiveOperation.Login;

            auth.InitializeAsync(onCompleted);
        }

        private void OnLoginCompleted(object sender, LoginCompletedEventArgs e)
        {
            if (e.Status == LiveConnectSessionStatus.Connected)
            {
                session = e.Session;
                client = new LiveConnectClient(session);
                client.GetCompleted += client_OperationCompleted;
                client.PostCompleted += client_OperationCompleted;
                client.DownloadCompleted += client_DownloadCompleted;
                client.UploadCompleted += client_UploadCompleted;

                if (String.IsNullOrWhiteSpace(ApplicationFolderID))
                {
                    CreateFolderHierarchyAsync(e.UserState);
                }
                else
                {
                    CurrentOperation = LiveOperation.GetApplicationFolderList;
                    client.GetAsync("/" + ApplicationFolderID + "/files", e.UserState);
                }
            }
            else
            {
                this.ErrorMessage = "Login error, try again later. " + ((e.Error != null) ? e.Error.ToString() : "");
                IsRunning = false;
                Action<LiveOperationCompletedEventArgs> onCompleted = e.UserState as Action<LiveOperationCompletedEventArgs>;
                if (onCompleted != null)
                {
                    onCompleted(null);
                }
            }

        }

        private void CreateFolderHierarchyAsync(object userstate)
        {
            currentIndex = 0;
            currentFolderId = null;

            CurrentOperation = LiveOperation.Login;
            client.GetAsync("me", userstate);
        }

        private void client_OperationCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                LiveConnectException liveException = e.Error as LiveConnectException;
                if (CurrentOperation == LiveOperation.GetApplicationFolderList && this.ApplicationFolderID != null && liveException.ErrorCode == "resource_not_found")
                {
                    ApplicationFolderID = null;
                    CreateFolderHierarchyAsync(e.UserState);
                }
                else
                {
                    ErrorMessage = "Communication error, try again later. " + e.Error.ToString();
                    IsRunning = false;
                    Action<LiveOperationCompletedEventArgs> onCompleted = e.UserState as Action<LiveOperationCompletedEventArgs>;
                    if (onCompleted != null)
                    {
                        onCompleted(e);
                    }
                }
                return;
            }

            switch (CurrentOperation)
            {
                case LiveOperation.Login:
                    UserName = e.Result["name"] as string;
                    CurrentOperation = LiveOperation.GetFolder;
                    client.GetAsync("/me/skydrive", e.UserState);
                    break;
                case LiveOperation.GetFolder:
                    currentFolderId = e.Result["id"] as string;
                    CurrentOperation = LiveOperation.GetFolderList;
                    client.GetAsync("/" + currentFolderId + "/files", e.UserState);
                    break;
                case LiveOperation.GetFolderList:
                    string previousFolderId = currentFolderId;
                    IList list = e.Result["data"] as IList;
                    string folderName = Path[currentIndex++];
                    IDictionary<string, object> folder = FindItemByName(folderName, list);
                    currentFolderId = folder != null ? folder["id"] as string : null;
                    if (currentFolderId == null)
                    {
                        CurrentOperation = LiveOperation.CreateFolder;
                        string parameter = "{\"name\" : \"" + folderName + "\", \"description\" : \"Your private data managed by apps running on your devices\"}";
                        client.PostAsync("/" + previousFolderId, parameter, e.UserState);
                    }
                    else if (currentIndex < Path.Count)
                    {
                        CurrentOperation = LiveOperation.GetFolderList;
                        client.GetAsync("/" + currentFolderId + "/files", e.UserState);
                    }
                    else
                    {
                        ApplicationFolderID = currentFolderId;
                        CurrentOperation = LiveOperation.GetApplicationFolderList;
                        client.GetAsync("/" + ApplicationFolderID + "/files", e.UserState);
                    }
                    break;
                case LiveOperation.CreateFolder:
                    currentFolderId = e.Result["id"] as string;
                    if (currentIndex < Path.Count)
                    {
                        CurrentOperation = LiveOperation.GetFolderList;
                        client.GetAsync("/" + currentFolderId + "/files", e.UserState);
                    }
                    else
                    {
                        ApplicationFolderID = currentFolderId;
                        CurrentOperation = LiveOperation.GetApplicationFolderList;
                        client.GetAsync("/" + ApplicationFolderID + "/files", e.UserState);
                    }
                    break;
                case LiveOperation.GetApplicationFolderList:
                    applicationFolderContent = e.Result["data"] as IList;
                    isRunning = false;
                    Action<LiveOperationCompletedEventArgs> onCompleted = e.UserState as Action<LiveOperationCompletedEventArgs>;
                    if (onCompleted != null)
                    {
                        onCompleted(e);
                    }
                    break;
            }
        }

        private static IDictionary<string, object> FindItemByName(string fileName, IList list)
        {
            foreach (object o in list)
            {
                IDictionary<string, object> item = o as IDictionary<string, object>;
                if (item["name"] as string != fileName) continue;
                return item;
            }
            return null;
        }

        public enum LiveOperation
        {
            Login,
            GetFolder,
            CreateFolder,
            GetFolderList,
            GetApplicationFolderList
        }

        public bool FileExists(string filename)
        {
            return FindItemByName(filename, this.applicationFolderContent) != null;
        }

        public DateTime GetFileUpdatedTime(string filename)
        {
            DateTime datetime = default(DateTime);
            var item = FindItemByName(filename, this.applicationFolderContent);
            if (item != null)
            {
                try
                {
                    datetime = Convert.ToDateTime(item["updated_time"]);
                }
                catch { }
            }
            return datetime;
        }

        public void DownloadAsync(string filename, Action<LiveDownloadCompletedEventArgs> onCompleted)
        {
            string fileID = FindItemByName(filename, this.applicationFolderContent)["id"] as string;
            if (fileID != null)
            {
                client.DownloadAsync(path: String.Format("/{0}/content", fileID), userState: onCompleted);
            }
            else
            {
                onCompleted(new LiveDownloadCompletedEventArgs(new FileNotFoundException("filename"), false, onCompleted));
            }
        }

        private void client_DownloadCompleted(object sender, LiveDownloadCompletedEventArgs e)
        {
            Action<LiveDownloadCompletedEventArgs> onCompleted = e.UserState as Action<LiveDownloadCompletedEventArgs>;
            if (onCompleted != null)
            {
                onCompleted(e);
            }
        }

        public void UploadAsync(string filename, Stream stream, Action<LiveOperationCompletedEventArgs> onCompleted)
        {
            client.UploadAsync(path: String.Format("/{0}", ApplicationFolderID), fileName: filename, 
                option:OverwriteOption.Overwrite, inputStream: stream, userState: onCompleted);
        }

        private void client_UploadCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            Action<LiveOperationCompletedEventArgs> onCompleted;
            onCompleted = e.UserState as Action<LiveOperationCompletedEventArgs>;
            if (onCompleted != null)
            {
                onCompleted(e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, e);
            }
        }

    }
}
