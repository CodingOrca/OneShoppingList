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
using System.Threading.Tasks;

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

        public async Task ConnectAsync()
        {
            if (IsRunning) return;
            auth = new LiveAuthClient(Secrets.ClientID);

            try
            {
                IsRunning = true;
                CurrentOperation = LiveOperation.Login;

                var e = await auth.InitializeAsync();
                if (e.Status != LiveConnectSessionStatus.Connected)
                {
                    this.ErrorMessage = "Login error, try again later. " + e.State;
                    throw new Exception(this.ErrorMessage);
                }
                session = e.Session;
                client = new LiveConnectClient(session);

                if (!String.IsNullOrWhiteSpace(ApplicationFolderID))
                {
                    CurrentOperation = LiveOperation.GetApplicationFolderList;
                    try
                    {
                        applicationFolderContent = await GetFolderList(ApplicationFolderID);
                        return;
                    }
                    catch (LiveConnectException ex)
                    {
                        if (ex.ErrorCode != "itemNotFound")
                        {
                            throw;
                        }
                        // if resource not found, we fall try to create the hierarchy again as if ApplicationFolderID would have been null
                        ApplicationFolderID = null;
                    }
                }
                await CreateFolderHierarchyAsync();
                applicationFolderContent = await GetFolderList(ApplicationFolderID);
            }
            finally
            {
                IsRunning = false;
            }
        }

        private async Task<IList> GetFolderList(string folderId)
        {
            var result = await client.GetAsync("/me/drive/items/" + folderId + "/children");
            return result.Result["value"] as IList;
        }

        private async Task CreateFolderHierarchyAsync()
        {
            currentIndex = 0;
            currentFolderId = null;

            CurrentOperation = LiveOperation.Login;
            var e = await client.GetAsync("/me");
            UserName = e.Result["userPrincipalName"] as string;

            CurrentOperation = LiveOperation.GetFolder;
            e = await client.GetAsync("/me/drive/root");
            var rootFolderId = e.Result["id"] as string;

            var appFolderId = await CreateSubfolderIfNotExists(rootFolderId, "AppData");
            var familyFolderId = await CreateSubfolderIfNotExists(appFolderId, "OneFamily");
            ApplicationFolderID = await CreateSubfolderIfNotExists(familyFolderId, "ShoppingList");
        }

        private async Task<string> CreateSubfolderIfNotExists(string parentFolderId, string subfolder)
        {
            CurrentOperation = LiveOperation.GetFolderList;
            var list = await GetFolderList(parentFolderId);
            var folderInfo = FindItemByName(subfolder, list);
            string subfolderId = folderInfo != null ? folderInfo["id"] as string : null;
            if (subfolderId == null)
            {
                subfolderId = await CreateFolder(parentFolderId, subfolder);
            }
            return subfolderId;
        }

        private async Task<string> CreateFolder(string parentFolderId, string folderName)
        {
            CurrentOperation = LiveOperation.CreateFolder;
            string parameter = "{\"name\" : \"" + folderName + "\", \"folder\": { }}";
            var e = await client.PostAsync("/me/drive/items/" + parentFolderId + "/children", parameter);
            return e.Result["id"] as string;
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
                    datetime = Convert.ToDateTime(item["lastModifiedDateTime"]);
                }
                catch { }
            }
            return datetime;
        }

        public async Task<Stream> DownloadAsync(string filename)
        {
            string fileID = FindItemByName(filename, this.applicationFolderContent)["id"] as string;
            if (fileID != null)
            {
                var downloadResult = await client.DownloadAsync(string.Format("/me/drive/items/{0}/content", fileID));
                return downloadResult.Stream;
            }
            return null;
        }

        public async Task UploadAsync(string filename, Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var body = reader.ReadToEnd();
                await client.PutAsync(String.Format("/me/drive/items/{0}:/{1}:/content", ApplicationFolderID, filename), body);
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
