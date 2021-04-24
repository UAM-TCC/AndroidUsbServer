using AndroidUsbServer.Models;
using AndroidUsbServer.Services;
using AndroidUsbServer.ViewModels;
using Hoho.Android.UsbSerial.Driver;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AndroidUsbServer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SetupPage : ContentPage
	{
		public SetupViewModel SetupViewModel => BindingContext as SetupViewModel;

		private TaskCompletionSource<AndroidServer> _modalTask;

		public SetupPage(TaskCompletionSource<AndroidServer> modalTask, UsbSerialPort usbPort)
		{
			InitializeComponent();
			BindingContext = new SetupViewModel(DependencyService.Get<IUsbService>(), usbPort);
			_modalTask = modalTask;
		}

		private void OnStartClicked(object sender, EventArgs e)
		{
			try
            {
				var setup = SetupViewModel.CreateSetup();
				_modalTask.SetResult(setup);
				Navigation.PopModalAsync();
            }
			catch (Exception ex)
            {
				SetupViewModel.ShowError(ex);
			}
		}

		private void OnCancelClicked(object sender, EventArgs e)
        {
			Navigation.PopModalAsync();
        }
    }
}