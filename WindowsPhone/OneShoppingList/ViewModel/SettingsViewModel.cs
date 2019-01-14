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
using System.IO.IsolatedStorage;
using System.Diagnostics;
using OneShoppingList.Resources;
using GalaSoft.MvvmLight;
using OneShoppingList.ViewModel;
using GalaSoft.MvvmLight.Command;
using OneShoppingList.Model;
using Microsoft.Phone.Net.NetworkInformation;

namespace OneShoppingList
{
    public class SettingsViewModel: ViewModelBase
    {
        // Our isolated storage settings
        IsolatedStorageSettings isolatedStore;

        // The isolated storage key names of our settings
        const string SyncEnabledKeyName = "SyncEnabled";
        const string LiveIdKeyName = "UserName";
        const string LastShoppingStoreKeyName = "LastShoppingStore";

        public AppResources Localized
        {
            get
            {
                return new LocalizedStrings().LocalizedResources;
            }
        }

        // default settings:
        bool syncEnabledDefault = true;
        string windowsLiveIDDefault = AppResources.notSignedIn;

        public bool IsUserKnown { get { return UserName != windowsLiveIDDefault; } }

        public bool SyncEnabled
        {
            get 
            {
                return GetValueOrDefault<bool>(SyncEnabledKeyName, this.syncEnabledDefault);
            }
            set 
            {
                AddOrUpdateValue(SyncEnabledKeyName, value);
                Save();
                base.RaisePropertyChanged("SyncEnabled");
            }
        }

        public string ClientId
        {
            get
            {
                return Secrets.ClientID;
            }
        }

        public int LastShoppingStore
        {
            get
            {
                return GetValueOrDefault<int>("LastShoppingStore", -1);
            }
            set
            {
                AddOrUpdateValue("LastShoppingStore", value);
                Save();
                base.RaisePropertyChanged("LastShoppingStore");
            }
        }

        public void RefreshIsTrial()
        {
            enforceNextTrialCheck = true;
            RaisePropertyChanged("IsTrial");
        }

        private bool enforceNextTrialCheck = true;
        private bool isTrial = true;
        public bool IsTrial
        {
            get
            {
                if( isTrial && enforceNextTrialCheck)
                {
                    var licenseinfo = new Microsoft.Phone.Marketplace.LicenseInformation();
                    isTrial = licenseinfo.IsTrial();
#if ENFORCE_TRIAL
                    isTrial = true;
#endif
                    if (IsInDesignMode)
                    {
                        isTrial = true;
                    }
                    this.enforceNextTrialCheck = false;
                }
                return isTrial;
            }
        }

        public bool ShowAllItemsInAllLists
        {
            get
            {
                return GetValueOrDefault<bool>("ShowAllItemsInAllLists", true);
            }
            set
            {
                AddOrUpdateValue("ShowAllItemsInAllLists", value);
                Save();
                base.RaisePropertyChanged("ShowAllItemsInAllLists");
            }
        }

        public int LastFavoritesPivotItem
        {
            get
            {
                return GetValueOrDefault<int>("LastFavoritesPivotItem", -1);
            }
            set
            {
                AddOrUpdateValue("LastFavoritesPivotItem", value);
                Save();
                base.RaisePropertyChanged("LastFavoritesPivotItem");
            }
        }

        public string ToggleButtonCaption
        {
            get
            {
                return this.ShowAllItemsInAllLists ? AppResources.showLessButtonCaption : AppResources.showAllButtonCaption;
            }
        }

        private RelayCommand toggleShowAllCommand;
        public RelayCommand ToggleShowAllCommand
        {
            get
            {
                if (this.toggleShowAllCommand == null)
                {
                    toggleShowAllCommand = new RelayCommand(
                        //Execute:
                        () =>
                        {
                            this.ShowAllItemsInAllLists = !this.ShowAllItemsInAllLists;

                            //foreach (Shop s in ViewModelLocator.Instance.Main.Shops)
                            //{
                            //    s.ShoppingListViewModel.RecreateViewModels();
                            //}

                            foreach (ShoppingItem item in ViewModelLocator.Instance.Main.AllItemsViewModel.ItemsView)
                            {
                                if (item.IsOnShoppingList)
                                {
                                    item.RaisePropertyChanged("Category");
                                }
                            }
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                foreach (Shop s in ViewModelLocator.Instance.Main.Shops)
                                {
                                    s.RefreshToggleButtonState();
                                }
                            });

                            this.RaisePropertyChanged("ToggleButtonCaption");
                        }
                        );
                }
                return toggleShowAllCommand;
            }
        }



        // TODO: 
        public bool ShowOnlyFavorites
        {
            get
            {
                return GetValueOrDefault<bool>("ShowOnlyFavorites", true);
            }
            set
            {
                AddOrUpdateValue("ShowOnlyFavorites", value);
                Save();
                base.RaisePropertyChanged("ShowOnlyFavorites");
            }
        }

        // TODO: 
        public bool IsFirstStart
        {
            get
            {
                return GetValueOrDefault<bool>("IsFirstStart", true);
            }
            set
            {
                AddOrUpdateValue("IsFirstStart", value);
                Save();
                base.RaisePropertyChanged("IsFirstStart");
            }
        }

        public string UserName
        {
            get 
            {
                return GetValueOrDefault<string>(LiveIdKeyName, this.windowsLiveIDDefault);
            }
            set
            {
                AddOrUpdateValue(LiveIdKeyName, value);
                Save();
                base.RaisePropertyChanged("UserName");
                base.RaisePropertyChanged("IsUserKnown");
                SyncCommand.RaiseCanExecuteChanged();
            }
        }

        public void LoggedOut()
        {
            UserName = windowsLiveIDDefault;
            ViewModelLocator locator = App.Current.Resources["Locator"] as ViewModelLocator;
            locator.Main.ResetSyncHandler();
        }

        public SettingsViewModel()
        {
            try
            {
                DeviceNetworkInformation.NetworkAvailabilityChanged += DeviceNetworkInformation_NetworkAvailabilityChanged;
                isolatedStore = IsolatedStorageSettings.ApplicationSettings;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: Cannot read Application Settings. ");
                Debug.WriteLine(e.ToString());
            }
        }

        public SyncHandler SyncHandler
        {
            get
            {
                return ViewModelLocator.Instance.Main.SyncHandler;
            }
        }

        public bool NetworkAvailable
        {
            get
            {
                return DeviceNetworkInformation.IsNetworkAvailable;
            }
        }

        private RelayCommand syncCommand;
        public RelayCommand SyncCommand
        {
            get
            {
                if( syncCommand == null )
                {
                    syncCommand = new RelayCommand(
                        () => // Execute:
                        {
                            this.SyncHandler.SyncAsync();
                        },
                        () => // CanExecute:
                        {
                            return !this.SyncHandler.IsRunning && DeviceNetworkInformation.IsNetworkAvailable && this.IsUserKnown;
                        }
                    );
                    this.SyncHandler.PropertyChanged += SyncHandler_PropertyChanged;
                }
                return syncCommand;
            }
        }

        void SyncHandler_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsRunning")
            {
                SyncCommand.RaiseCanExecuteChanged();
            }
        }

        public void RefreshAllNotofications()
        {
            this.RaisePropertyChanged("NetworkAvailable");
            this.RaisePropertyChanged("IsTrial");
            this.RaisePropertyChanged("IsUserKnown");
            this.RaisePropertyChanged("UserName");
            this.RaisePropertyChanged("ShowOnlyFavorites");
            this.RaisePropertyChanged("ShowAllItemsInAllLists");
            this.RaisePropertyChanged("SyncEnabled");

            ToggleShowAllCommand.RaiseCanExecuteChanged();
            SyncCommand.RaiseCanExecuteChanged();
        }

        void DeviceNetworkInformation_NetworkAvailabilityChanged(object sender, NetworkNotificationEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    base.RaisePropertyChanged("NetworkAvailable");
                    SyncCommand.RaiseCanExecuteChanged();
                });
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key">The Key Name of the setting, e.g QuizModeEnabledKeyName</param>
        /// <param name="value">The value to be saved for that setting</param>
        /// <returns></returns>
        private bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (isolatedStore.Contains(Key))
            {
                // If the value has changed
                if (isolatedStore[Key] != value)
                {
                    // Store the new value
                    isolatedStore[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                isolatedStore.Add(Key, value);
                valueChanged = true;
            }

            return valueChanged;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private valueType GetValueOrDefault<valueType>(string Key, valueType defaultValue)
        {
            if (this.IsInDesignMode)
            {
                return defaultValue;
            }

            valueType value;

            // If the key exists, retrieve the value.
            if (isolatedStore.Contains(Key))
            {
                value = (valueType)isolatedStore[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Save the settings.
        /// </summary>
        public void Save()
        {
            isolatedStore.Save();
        }
    }
}
