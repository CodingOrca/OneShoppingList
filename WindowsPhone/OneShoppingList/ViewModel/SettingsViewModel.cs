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
                isolatedStore = IsolatedStorageSettings.ApplicationSettings;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: Cannot read Application Settings. ");
                Debug.WriteLine(e.ToString());
            }
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
