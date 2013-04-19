using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
namespace Framework
{
    // This code writen by Wieser Software Ltd 
    // www.wieser-software.com  
    // Use of this code is permitted as is or in derived 
    // works provided the code is attributed to us.  
    public static class WPHacks
    {
        static Thickness PortraitMargin = new Thickness(0, 32, 0, 0);
        static Thickness MarginLandscapeLeft = new Thickness(72, 0, 0, 0);
        static Thickness MarginLandscapeRight = new Thickness(0, 0, 72, 0);
        /// <summary>    
        /// Wire up our fixes for SystemTray visibility    
        /// Make sure you call this after you've called     
        /// InitializeComponent.    
        /// </summary>    
        /// <param name="page">The page to fix</param>    
        public static void WireOrientationHack(PhoneApplicationPage page)
        {
            if (!SystemTray.GetIsVisible(page)) return;
            // if using the SystemTray,      
            // set up the initial margin based on the       
            // page's desired orientation     
            PageOrientation o = page.Orientation;
            OnOrientationChanged(page, new OrientationChangedEventArgs(o));
            // you may be tempted to use the SystemTray.Opacity       
            // property instead, but you'd be wrong, because that       
            // is asking what the current SystemTray is showing     
            // not what this page wants it to be set to      
            if (SystemTray.GetOpacity(page) == 1.0)
            {
                SystemTray.SetOpacity(page, 0.0);
            }

            page.OrientationChanged += OnOrientationChanged;
        }
        /// <summary>   
        /// /// On an orientation change, we readjust the margins   
        /// /// on the page, to leave room for the system tray.      
        /// 32 Pixels on top for portrait, 72 pixels on left     
        /// for LandscapeLeft, 72 pixels on right for LandscapeRight    
        /// </summary>    
        /// <param name="sender">The page changing orientation</param>    
        /// <param name="e">Event args</param>    
        static void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            PhoneApplicationPage src = sender as PhoneApplicationPage;
            if (src == null) return;
            if (0 != (e.Orientation & PageOrientation.Portrait))
            {
                src.Margin = PortraitMargin;
            }
            else
            {
                src.Margin = ((e.Orientation & PageOrientation.LandscapeLeft) == PageOrientation.LandscapeLeft) ? MarginLandscapeLeft : MarginLandscapeRight;
            }
        }
    }
}