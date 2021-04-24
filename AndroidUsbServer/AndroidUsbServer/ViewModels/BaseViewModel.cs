using AndroidUsbServer.Common;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace AndroidUsbServer.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void Subscribe<TArgs>(string message, Action<Application, TArgs> callback)
        {
            MessagingCenter.Subscribe<Application, TArgs>(Application.Current, message, callback);
        }

        public void Send<TArgs>(string message, TArgs args)
        {
            MessagingCenter.Send<Application, TArgs>(Application.Current, message, args);
        }

        public void Unsubscribe<TArgs>(string message)
        {

            MessagingCenter.Unsubscribe<Application, TArgs>(Application.Current, message);
        }

        public void ShowMessage(string msg, string title = "Info")
        {
            Device.InvokeOnMainThreadAsync(async () => await Application.Current.MainPage.DisplayAlert(title, msg, "Ok"));
        }

        public void ShowError(Exception ex)
        {
            Device.InvokeOnMainThreadAsync(async () => await Application.Current.MainPage.DisplayAlert("Error", ex.ToErrorString(), "Ok"));
        }
    }
}
