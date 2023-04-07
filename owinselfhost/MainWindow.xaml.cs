using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Net;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using System.Net.Http;
using Microsoft.Owin.Cors;
using Newtonsoft.Json;
using ReverseProxyEg;

namespace owinselfhost
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string baseAddress;
        SimpleFileLogger logger = new SimpleFileLogger("mylog.txt");
        
        public MainWindow()
        {
            InitializeComponent();
            logger.Log("initi");
        }
        StartOptions options = new StartOptions();
        private void ButtonHost_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                baseAddress = "http://"+txtHost.Text.Trim().ToLower() + ":" + txtProt.Text.Trim()+"/";
                //var options = new StartOptions("http://*:"+ txtProt.Text.Trim())
                //{
                //    ServerFactory = "Microsoft.Owin.Host.HttpListener"
                //};

                //options.Urls.Add($"http://localhost:{txtProt.Text.Trim()}");
                //options.Urls.Add($"http://127.0.0.1:{txtProt.Text.Trim()}");
                //options.Urls.Add($"http://{Environment.MachineName}:{txtProt.Text.Trim()}" );


                //WebApp.Start<MilkyWebServerStartup>(options: options);
                //web = WebApp.Start<MilkyWebServerStartup>(url: baseAddress);
                // baseAddress = "http://+:9089/";
                web=WebApp.Start<MilkyWebServerStartup>(baseAddress);
                 
               
                var port = Convert.ToInt32(txtProt.Text.Trim());
                Task.Run(() =>
                {
                    SimpleProxy proxy = new SimpleProxy(port + 1, "127.0.0.1", port, logger);
                    proxy.Start();
                });
                
                TextBlockStatus.Text = "STARETED at" + baseAddress;
                foreach (var startOption in options.Urls)
                {
                    TextBlockStatus.Text += Environment.NewLine + startOption;
                }
            }
            catch (Exception exception)
            {

                TextBlockStatus.Text = JsonConvert.SerializeObject(exception);
            }
        }

        public IDisposable web
        { get; set; }

        private void ButtonText_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                
                using HttpClient client = new HttpClient();
                var url = txtHostClinet.Text.Trim();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("MilkyServerAuthorization", "saha/sarvani");
                var response = client.GetAsync(url).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                TextBlockStatus.Text = responseString + DateTime.Now.Ticks;          
            }
            catch (Exception exception)
            {
                TextBlockStatus.Text = JsonConvert.SerializeObject(exception);
            }
        }

        private void AdditionalHOstProt_OnClick(object sender, RoutedEventArgs e)
        {
                options.Urls.Add($"http://{txtAddiitonalHost.Text.Trim()}:{txtProt.Text.Trim()}");

        }

        private void BTNsHOomeOptions_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var startOption in options.Urls)
            {
                TextBlockStatus.Text += Environment.NewLine + startOption;
            }
        }
    }

    public class MilkyWebServerStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();
            appBuilder.UseCors(CorsOptions.AllowAll);
            config.MapHttpAttributeRoutes(); //Don't miss this

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            ((Newtonsoft.Json.Serialization.DefaultContractResolver)config.Formatters.JsonFormatter.SerializerSettings.ContractResolver).IgnoreSerializableAttribute = true;
            appBuilder.UseWebApi(config);
        }
    }
}
