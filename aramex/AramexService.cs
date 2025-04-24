using Microsoft.Extensions.Options;
using AramexShippingService;
using System.Threading.Tasks;

namespace aramex
{
    public class AramexService
    {
        private readonly string _userEmail;
        private readonly string _password;
        private readonly string _version;
        private readonly string _accountNumber;
        private readonly string _accountPin;
        private readonly string _accountEntity;
        private readonly string _accountCountryCode;
        public AramexService(IConfiguration configuration)
        {
            _userEmail = configuration["AramexSettings:UserName"];
            _password = configuration["AramexSettings:UserPassword"];
            _version = configuration["AramexSettings:Version"];
            _accountNumber = configuration["AramexSettings:AccountNumber"];
            _accountPin = configuration["AramexSettings:AccountPin"];
            _accountEntity = configuration["AramexSettings:Entity"];
            _accountCountryCode = configuration["AramexSettings:CountryCode"];

        }

        public ClientInfo GetAccountInfo()
        {
            return new ClientInfo
            {
                AccountCountryCode = "SA",
                AccountEntity = "JED",
                AccountNumber = "72075454",
                AccountPin = "236377",
                Password = "A789n987@",
                Version = "v1.0",
                UserName = "awjtek@gmail.com"
            
            };
        }

        public async Task<AramexResponse> CreateShipment(SimpleShipmentRequest request)
        {
            var _client = new Service_1_0Client(Service_1_0Client.EndpointConfiguration.BasicHttpBinding_Service_1_01, "https://ws.aramex.net/ShippingAPI.V2/Shipping/Service_1_0.svc?wsdl");
            var shipment = new Shipment
            {
                Shipper = new Party
                {
                    Contact = new Contact
                    {
                        PersonName = request.SenderName,
                        PhoneNumber1 = request.SenderPhone,
                        CellPhone = request.SenderPhone,
                    },
                    PartyAddress = new Address
                    {
                        City = request.SenderCity,
                        Line1 = request.SenderDistrict,
                        CountryCode = "SA" // Saudi Arabia by default
                    }
                },
                Consignee = new Party
                {
                    Contact = new Contact
                    {
                        PersonName = request.ReceiverName,
                        PhoneNumber1 = request.ReceiverPhone,
                        CellPhone = request.ReceiverPhone,
                    },
                    PartyAddress = new Address
                    {
                        City = request.ReceiverCity,
                        Line1 = request.ReceiverDistrict,
                        CountryCode = "SA" // Saudi Arabia by default
                    }
                },
                ShippingDateTime = DateTime.Now,
                Details = new ShipmentDetails
                {
                    DescriptionOfGoods = request.ShipmentType,
                    ActualWeight = new Weight
                    {
                        Value = (double)request.WeightKg,
                        Unit = "Kg"
                    },
                    Dimensions = new Dimensions
                    {
                        Length = (double)request.LengthCm,
                        Width = (double)request.WidthCm,
                        Height = (double)request.HeightCm,
                        Unit = "cm"
                    },
                    ProductGroup = "EXP", // Express
                    ProductType = "PDX",  // Parcel
                    PaymentType = "P",     // Prepaid
                    NumberOfPieces = 1
                }
            };
            var info = GetAccountInfo();
            Console.WriteLine($"Entity: {info.AccountEntity}, Number: {info.AccountNumber}, PIN: {info.AccountPin}, Country: {info.AccountCountryCode}, {info.UserName}");

            var response = await _client.CreateShipmentsAsync(new ShipmentCreationRequest
            {
                ClientInfo = GetAccountInfo(),
                Shipments = new[] { shipment }
            });
            if (response.HasErrors)
            {
                foreach (var notification in response.Notifications)
                {
                    Console.WriteLine($"❗ Aramex Error: {notification.Code} - {notification.Message}");
                }
            }
            else
            {
                Console.WriteLine("Shipment created successfully.");
                Console.WriteLine("Shipment ID: " + response.Shipments?.FirstOrDefault()?.ID);
            }
            Console.WriteLine("Shipment Response: ");
            Console.WriteLine("Shipments count: " + response.Shipments?.Count());
            if (response.Shipments != null && response.Shipments.Any())
            {
                Console.WriteLine("Shipment Number: " + response.Shipments.First().ID);
            }
            else
            {
                Console.WriteLine("No shipments returned.");
            }

            return new AramexResponse
            {
                ShipmentNumber = response.Shipments?.FirstOrDefault()?.ID,
            };
        }
    }

    public class SimpleShipmentRequest
    {
        // Sender Information
        public string SenderName { get; set; }
        public string SenderCity { get; set; }
        public string SenderDistrict { get; set; }
        public string SenderPhone { get; set; }

        // Receiver Information
        public string ReceiverName { get; set; }
        public string ReceiverCity { get; set; }
        public string ReceiverDistrict { get; set; }
        public string ReceiverPhone { get; set; }

        // Shipment Details
        public string ShipmentType { get; set; }
        public decimal WeightKg { get; set; }
        public decimal LengthCm { get; set; }
        public decimal WidthCm { get; set; }
        public decimal HeightCm { get; set; }
    }

    public class AramexResponse
    {
        public string ShipmentNumber { get; set; }
        public string LabelUrl { get; set; }
    }
}