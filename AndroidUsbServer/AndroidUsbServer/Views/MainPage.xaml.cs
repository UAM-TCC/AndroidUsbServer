using AndroidUsbServer.Models;
using AndroidUsbServer.Services;
using AndroidUsbServer.ViewModels;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AndroidUsbServer.Views
{
    public partial class MainPage : ContentPage
    {
        public MainViewModel UsbListViewModel => BindingContext as MainViewModel;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainViewModel(DependencyService.Get<IUsbService>(), DependencyService.Get<IServer>());
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                await UsbListViewModel.RefreshUsbList();
            }
            catch (Exception ex)
            {
                UsbListViewModel.ShowError(ex);
            }

            UsbListViewModel.Subscribe<string>("Intent", async (sender, args) =>
            {
                try
                {
                    await UsbListViewModel.RefreshUsbList();
                }
                catch (Exception ex)
                {
                    UsbListViewModel.ShowError(ex);
                }
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            UsbListViewModel.Unsubscribe<string>("Intent");

            UsbListViewModel.Disconnect();
        }

        private async void UsbList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                var port = UsbListViewModel.GetUsbPort(e.ItemIndex);

                var modalTask = new TaskCompletionSource<AndroidServer>();
                var setupPage = new SetupPage(modalTask, port);
                await Navigation.PushModalAsync(setupPage);
                var server = await modalTask.Task;

                server = await UsbListViewModel.ConnectAsync(server);
                UsbListViewModel.ShowMessage(server.Endpoint.ToString());
            }
            catch (Exception ex)
            {
                UsbListViewModel.ShowError(ex);
            }
        }
    }
}
