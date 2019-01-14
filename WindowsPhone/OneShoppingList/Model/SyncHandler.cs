using Microsoft.Live;
using Microsoft.Phone.Net.NetworkInformation;
using OneShoppingList.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OneShoppingList.Model
{
    public class SyncHandler : INotifyPropertyChanged
    {
        private SkyDriveHandler skyDriveHandler;
        private CultureInfo german;

        private List<string> filesToSync = new List<string>()
        {
            "OneShoppingStores.txt",
            "OneShoppingList.txt"
        };

        private IsolatedStorageSettings isolatedStorageSettings;


        public SyncHandler()
        {
            german = new CultureInfo("de-DE");
            skyDriveHandler = new SkyDriveHandler();
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

        private string LastShopsUpdatedTimeSettingsKey = "LastShopsUpdatedTime";
        private DateTime LastShopsUpdatedTime
        {
            get
            {
                if (isolatedStorageSettings.Contains(LastShopsUpdatedTimeSettingsKey))
                {
                    return Convert.ToDateTime(isolatedStorageSettings[LastShopsUpdatedTimeSettingsKey], german);
                }
                else
                {
                    return default(DateTime);
                }
            }
            set
            {
                if (isolatedStorageSettings.Contains(LastShopsUpdatedTimeSettingsKey))
                {
                    isolatedStorageSettings[LastShopsUpdatedTimeSettingsKey] = value.ToString(german);
                    isolatedStorageSettings.Save();
                }
                else
                {
                    isolatedStorageSettings.Add(LastShopsUpdatedTimeSettingsKey, value.ToString(german));
                    isolatedStorageSettings.Save();
                }
            }
        }

        // the local time when the last sync was performed.
        private string LastSyncTimeSettingsKey = "LastSyncTime";
        public DateTime LastSyncTime
        {
            get
            {
                if (isolatedStorageSettings.Contains(LastSyncTimeSettingsKey))
                {
                    return Convert.ToDateTime(isolatedStorageSettings[LastSyncTimeSettingsKey], german);
                }
                else
                {
                    return default(DateTime);
                }
            }
            set
            {
                if (isolatedStorageSettings.Contains(LastSyncTimeSettingsKey))
                {
                    if (value.ToString(german) != isolatedStorageSettings[LastSyncTimeSettingsKey] as string)
                    {
                        isolatedStorageSettings[LastSyncTimeSettingsKey] = value.ToString(german);
                        isolatedStorageSettings.Save();
                    }
                }
                else
                {
                    isolatedStorageSettings.Add(LastSyncTimeSettingsKey, value.ToString(german));
                    isolatedStorageSettings.Save();
                }
            }
        }

        // the latest synced skydrive timestamp of the Product List file
        private string LastProductListTimeStampSettingsKey = "LastProductListTimeStamp";
        private DateTime LastProductListTimeStamp
        {
            get
            {
                if (isolatedStorageSettings.Contains(LastProductListTimeStampSettingsKey))
                {
                    return Convert.ToDateTime(isolatedStorageSettings[LastProductListTimeStampSettingsKey], german);
                }
                else
                {
                    return default(DateTime);
                }
            }
            set
            {
                if (isolatedStorageSettings.Contains(LastProductListTimeStampSettingsKey))
                {
                    if (value.ToString(german) != isolatedStorageSettings[LastProductListTimeStampSettingsKey] as string)
                    {
                        isolatedStorageSettings[LastProductListTimeStampSettingsKey] = value.ToString(german);
                        isolatedStorageSettings.Save();
                    }
                }
                else
                {
                    isolatedStorageSettings.Add(LastProductListTimeStampSettingsKey, value.ToString(german));
                    isolatedStorageSettings.Save();
                }
            }
        }

        public void Reset()
        {
            this.LastShopsUpdatedTime = default(DateTime);
            this.LastSyncTime = default(DateTime);
            this.LastProductListTimeStamp = default(DateTime);
            skyDriveHandler.Reset();
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
                if (isRunning == false)
                {
                    this.CurrentOperation = SyncOperation.Done;
                    if (!DeviceNetworkInformation.IsNetworkAvailable)
                    {
                        this.errorMessage = AppResources.NoNetworkAvailable;
                    }
                    if (!String.IsNullOrEmpty(this.errorMessage))
                    {
                        MessageBox.Show(this.errorMessage, "ERROR", MessageBoxButton.OK);
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                RaisePropertyChanged(IsRunningPropertyName);
            }
        }

        private SyncOperation currentOperation;
        public SyncOperation CurrentOperation
        {
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

        public List<ShoppingItem> ProductItems { get; private set; }

        public List<Shop> Shops { get; private set; }



        public enum SyncOperation
        {
            Syncing,
            Downloading,
            Merging,
            Uploading,
            Done
        }

        private string errorMessage;

        public async Task SyncAsync()
        {
            bool repeat = false;
            int repeatCount = 0;
            do
            {
                repeatCount++;
                try
                {
                    this.errorMessage = null;
                    this.IsRunning = true;
                    this.Shops = null;
                    this.ProductItems = null;

                    this.CurrentOperation = SyncOperation.Syncing;
                    await skyDriveHandler.ConnectAsync();
                    var stream = await DownloadAsync("OneShoppingStores.txt", LastShopsUpdatedTime);
                    if (stream != null)
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Shop>));
                        object o = ser.ReadObject(stream);
                        this.Shops = o as List<Shop>;
                    }
                    stream = await DownloadAsync("OneShoppingList.txt", LastProductListTimeStamp);
                    if (stream != null)
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<ShoppingItem>));
                        object o = ser.ReadObject(stream);
                        this.ProductItems = o as List<ShoppingItem>;

                    }

                    this.CurrentOperation = SyncOperation.Merging;
                    var syncTransationTime = DateTime.Now;
                    MergeAllListsSync();

                    if (this.Shops != null)
                    {
                        this.CurrentOperation = SyncOperation.Uploading;
                        await UploadAsync(this.Shops, filesToSync[0]);
                    }
                    if (this.ProductItems != null)
                    {
                        this.CurrentOperation = SyncOperation.Uploading;
                        await UploadAsync(this.ProductItems, filesToSync[1]);
                    }

                    this.CurrentOperation = SyncOperation.Syncing;

                    // if at least one file was uploaded, get the file list again so that we save the time stamps of oth files.
                    if (this.Shops != null || this.ProductItems != null)
                    {
                        await skyDriveHandler.ConnectAsync();

                        this.LastProductListTimeStamp = skyDriveHandler.GetFileUpdatedTime(filesToSync[1]);
                        this.LastShopsUpdatedTime = skyDriveHandler.GetFileUpdatedTime(filesToSync[0]);
                    }

                    this.LastSyncTime = syncTransationTime;

                    this.CurrentOperation = SyncOperation.Done;
                    this.IsRunning = false;
                }
                catch (LiveConnectException ex)
                {
                    if (ex.ErrorCode == "InvalidAuthenticationToken")
                    {
                        repeat = true;
                    }
                }
                catch (Exception ex)
                {
                    this.errorMessage = AppResources.connectionErrorMessage + ex.Message;
                }
                finally
                {
                    IsRunning = false;
                }
                if (repeat && repeatCount <= 1)
                {
                    var authClient = new LiveAuthClient(Secrets.ClientID);
                    authClient.Logout();
                    await authClient.LoginAsync(new string[] { "Files.ReadWrite", "offline_access", "User.Read" });
                }
            }
            while (repeat && repeatCount <= 1);
        }

        private async Task<Stream> DownloadAsync(string fileName, DateTime lastUpdatedTime)
        {
            if (skyDriveHandler.FileExists(fileName))
            {
                DateTime actualTime = skyDriveHandler.GetFileUpdatedTime(fileName);
                var deltaTime = actualTime - lastUpdatedTime;
                if (Math.Abs(deltaTime.TotalSeconds) > 1)
                {
                    this.CurrentOperation = SyncOperation.Downloading;
                    return await skyDriveHandler.DownloadAsync(fileName);
                }
            }
            return null;
        }

        private void MergeAllListsSync()
        {
            if (this.ProductItems != null)
            {
                bool uploadNeeded = ListSync.SyncLists(DataLocator.Current.ProductItems, this.ProductItems);
                this.LastProductListTimeStamp = skyDriveHandler.GetFileUpdatedTime(filesToSync[1]);
                if (!uploadNeeded)
                {
                    this.ProductItems = null;
                }
            }
            else
            {
                DateTime lastChange = DataLocator.Current.ProductItems.Count > 0 
                    ? DataLocator.Current.ProductItems.Max(item => item.TimeStamp) 
                    : default(DateTime);

                if (!skyDriveHandler.FileExists(filesToSync[1]) || lastChange > LastSyncTime)
                {
                    this.ProductItems = DataLocator.Current.ProductItems.Select(item => item.Clone() as ShoppingItem).ToList();
                }
            }

            if (this.Shops != null)
            {
                bool uploadNeeded = ListSync.SyncLists(DataLocator.Current.Shops, this.Shops);
                this.LastShopsUpdatedTime = skyDriveHandler.GetFileUpdatedTime(filesToSync[0]);
                if (!uploadNeeded)
                {
                    this.Shops = null;
                }
            }
            else
            {
                DateTime lastChange = DataLocator.Current.Shops.Count > 0
                ? DataLocator.Current.Shops.Max(shop => shop.TimeStamp)
                : default(DateTime);

                if (!skyDriveHandler.FileExists(filesToSync[0]) || lastChange > LastSyncTime)
                {
                    this.Shops = DataLocator.Current.Shops.Select(shop => shop.Clone() as Shop).ToList();
                }
            }

            DataLocator.Current.SaveLocalData();
        }

        private async Task UploadAsync(Object listToUpload, string fileName)
        {
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(listToUpload.GetType());
            ser.WriteObject(ms, listToUpload);
            ms.Seek(0, SeekOrigin.Begin);
            await skyDriveHandler.UploadAsync(fileName, ms);
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
