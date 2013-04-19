using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.Live;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Globalization;
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

        private DateTime GetLastUpdatedTimeOf(int fileIndex)
        {
            return fileIndex == 0 ? LastShopsUpdatedTime : LastProductListTimeStamp;
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
                    if (value.ToString(german) != isolatedStorageSettings[LastShopsUpdatedTimeSettingsKey] as string)
                    {
                        isolatedStorageSettings[LastShopsUpdatedTimeSettingsKey] = value.ToString(german);
                        isolatedStorageSettings.Save();
                    }
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
            Uploading
        }

        public void SyncAsync()
        {
            this.IsRunning = true;
            this.Shops = null;
            this.ProductItems = null;

            this.CurrentOperation = SyncOperation.Syncing;
            skyDriveHandler.ConnectAsync(e =>
                {
                    if (e != null && e.Result != null)
                    {
                        this.CurrentOperation = SyncOperation.Downloading;
                        DownloadAsync(0);
                    }
                    else
                    {
                        IsRunning = false;
                    }
                }
            );
        }

        private void DownloadAsync(int fileIndex)
        {
            if (fileIndex >= filesToSync.Count)
            {
                this.CurrentOperation = SyncOperation.Merging;
                MergeAllListsSync();
            }
            else
            {
                bool downloadStarted = false;
                if (skyDriveHandler.FileExists(filesToSync[fileIndex]))
                {
                    DateTime actualTime = skyDriveHandler.GetFileUpdatedTime(filesToSync[fileIndex]);
                    DateTime lastUpdatedTime = GetLastUpdatedTimeOf(fileIndex);
                    if (actualTime > lastUpdatedTime)
                    {
                        skyDriveHandler.DownloadAsync(filesToSync[fileIndex], e =>
                            {
                                client_DownloadCompleted(fileIndex, e);
                            }
                        );
                        downloadStarted = true;
                    }
                }
                if (!downloadStarted)
                {
                    DownloadAsync(fileIndex + 1);
                }
            }
        }

        private void client_DownloadCompleted(int fileIndex, LiveDownloadCompletedEventArgs e)
        {
            if ( e != null && !e.Cancelled && e.Error == null)
            {
                switch (fileIndex)
                {
                    case 0:
                        {
                            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Shop>));
                            object o = ser.ReadObject(e.Result);
                            this.Shops = o as List<Shop>;
                            break;
                        }
                    case 1:
                        {
                            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<ShoppingItem>));
                            object o = ser.ReadObject(e.Result);
                            this.ProductItems = o as List<ShoppingItem>;
                            break;
                        }
                }
            }
            DownloadAsync(fileIndex + 1);
        }

        private DateTime syncTransationTime;

        private void MergeAllListsSync()
        {
            syncTransationTime = DateTime.Now;
            if (this.ProductItems != null)
            {
                DateTime lastUpdatedTime = GetLastUpdatedTimeOf(1);
                bool uploadNeeded = ListSync.SyncLists(DataLocator.Current.ProductItems, this.ProductItems);
                this.LastProductListTimeStamp = skyDriveHandler.GetFileUpdatedTime(filesToSync[1]);
                if (!uploadNeeded)
                {
                    this.ProductItems = null;
                }
            }
            else
            {
                DateTime lastChange = default(DateTime);
                foreach (ShoppingItem item in DataLocator.Current.ProductItems)
                {
                    if (item.TimeStamp > lastChange)
                    {
                        lastChange = item.TimeStamp;
                    }
                }
                if (!skyDriveHandler.FileExists(filesToSync[1]) || lastChange > LastSyncTime)
                {
                    this.ProductItems = new List<ShoppingItem>();
                    foreach (ShoppingItem item in DataLocator.Current.ProductItems)
                    {
                        this.ProductItems.Add(item.Clone() as ShoppingItem);
                    }
                }
            }

            if (this.Shops != null)
            {
                DateTime lastUpdatedTime = GetLastUpdatedTimeOf(0);
                bool uploadNeeded = ListSync.SyncLists(DataLocator.Current.Shops, this.Shops);
                this.LastShopsUpdatedTime = skyDriveHandler.GetFileUpdatedTime(filesToSync[0]);
                if (!uploadNeeded)
                {
                    this.Shops = null;
                }
            }
            else
            {
                DateTime lastChange = default(DateTime);
                foreach (Shop item in DataLocator.Current.Shops)
                {
                    if (item.TimeStamp > lastChange)
                    {
                        lastChange = item.TimeStamp;
                    }
                }
                if (!skyDriveHandler.FileExists(filesToSync[0]) || lastChange > LastSyncTime)
                {
                    this.Shops = new List<Shop>();
                    foreach (Shop item in DataLocator.Current.Shops)
                    {
                        this.Shops.Add(item.Clone() as Shop);
                    }
                }
            }

            if (this.Shops != null || this.ProductItems != null)
            {
                this.CurrentOperation = SyncOperation.Uploading;
                this.UploadAsync(0);
            }
            else
            {
                this.LastProductListTimeStamp = skyDriveHandler.GetFileUpdatedTime(filesToSync[1]);
                this.LastShopsUpdatedTime = skyDriveHandler.GetFileUpdatedTime(filesToSync[0]);
                this.LastSyncTime = syncTransationTime;
                this.IsRunning = false;
            }
        }

        private void UploadAsync(int fileIndex)
        {
            if (fileIndex >= filesToSync.Count)
            {
                this.CurrentOperation = SyncOperation.Syncing;
                skyDriveHandler.ConnectAsync(ea => UpdateTimestamps(ea));
            }
            else
            {
                bool uploadStarted = false;
                switch (fileIndex)
                {
                    case 0:
                        if (this.Shops != null)
                        {
                            MemoryStream ms = new MemoryStream();
                            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Shop>));
                            ser.WriteObject(ms, this.Shops);
                            ms.Seek(0, SeekOrigin.Begin);
                            skyDriveHandler.UploadAsync(filesToSync[fileIndex], ms, e =>
                                {
                                    client_UploadCompleted(fileIndex, e);
                                }
                            );
                            uploadStarted = true;
                        }
                        break;
                    case 1:
                        if (this.ProductItems != null)
                        {
                            MemoryStream ms = new MemoryStream();
                            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<ShoppingItem>));
                            ser.WriteObject(ms, this.ProductItems);
                            ms.Seek(0, SeekOrigin.Begin);
                            skyDriveHandler.UploadAsync(filesToSync[fileIndex], ms, e =>
                                {
                                    client_UploadCompleted(fileIndex, e);
                                }
                            );
                            uploadStarted = true;
                        }
                        break;
                }
                if (!uploadStarted)
                {
                    UploadAsync(fileIndex + 1);
                }
            }
        }

        private void SaveLocalData()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = isf.OpenFile("OneShoppingList.txt", FileMode.Create))
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<ShoppingItem>));
                    ser.WriteObject(stream, new List<ShoppingItem>(this.ProductItems));
                }
                using (IsolatedStorageFileStream stream = isf.OpenFile("OneShoppingStores.txt", FileMode.Create))
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Shop>));
                    ser.WriteObject(stream, new List<Shop>(this.Shops));
                }
            }
        }

        private void client_UploadCompleted(int fileIndex, LiveOperationCompletedEventArgs e)
        {
            if (e != null && !e.Cancelled && e.Error == null)
            {
            }
            UploadAsync(fileIndex + 1);
        }

        private void UpdateTimestamps(LiveOperationCompletedEventArgs e)
        {
            if (e != null && !e.Cancelled && e.Error == null)
            {
                this.LastSyncTime = syncTransationTime;
                if (this.ProductItems != null && skyDriveHandler.FileExists(filesToSync[1]))
                {
                    this.LastProductListTimeStamp = skyDriveHandler.GetFileUpdatedTime(filesToSync[1]);
                }
                if (this.Shops != null && skyDriveHandler.FileExists(filesToSync[0]))
                {
                    this.LastShopsUpdatedTime = skyDriveHandler.GetFileUpdatedTime(filesToSync[0]);
                }
            }
            IsRunning = false;
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
