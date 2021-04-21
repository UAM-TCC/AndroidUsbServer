using AndroidUsbServer.Services;
using AndroidUsbServer.Utils;
using AndroidUsbServer.ViewModels;
using System;
using Xamarin.Forms;

namespace AndroidUsbServer
{
    public partial class MainPage : ContentPage
    {
        public UsbPortViewModel UsbPortViewModel => BindingContext as UsbPortViewModel;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = new UsbPortViewModel(this, DependencyService.Get<IUsbService>(), DependencyService.Get<IServer>());

            MessagingCenter.Subscribe<Application, string>(Application.Current, "Intent", (sender, args) =>
            {
                ShowMessage(args);
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                await UsbPortViewModel.InitAsync();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<Application, string>(Application.Current, "Intent");

            UsbPortViewModel.Disconnect();
        }

        private async void UsbList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                var server = await UsbPortViewModel.ConnectAsync(e.ItemIndex);
                ShowMessage(server.Endpoint.ToString());
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        public void ShowMessage(string msg, string title = "Info")
        {
            Device.InvokeOnMainThreadAsync(async () => await DisplayAlert(title, msg, "Ok"));
        }

        public void ShowError(Exception ex)
        {
            Device.InvokeOnMainThreadAsync(async () => await DisplayAlert("Error", Util.ErrorString(ex), "Ok"));
        }
    }
}
