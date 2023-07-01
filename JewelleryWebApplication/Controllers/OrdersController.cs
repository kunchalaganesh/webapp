﻿
using JewelleryWebApplication.Interface;
using JewelleryWebApplication.Models;
using JewelleryWebApplication.Models.ViewModel;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net.Mail;
using System.Net;
using MailKit;
using System.Security.Cryptography;
using ContentDisposition = MimeKit.ContentDisposition;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using iText.Html2pdf;
using System.IO;
using iText.Commons.Actions.Data;
using JewelleryWebApplication.Models.APIModel;
using Castle.Core.Resource;

namespace JewelleryWebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {  // public readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly IOrdersRepository _ordersRepository;
        private readonly ICustomerDetailsRepository _customerDetailsRepository;
        private readonly IRateRepository _rateRepository;
        private readonly IPurityRepository _purityRepository;
        private readonly IMaterialCategoryRepository _materialCategoryRepository;
        private readonly IProductTypeRepository _productTypeRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IProductRepository _productrepository;
        private readonly IEmailSender _emailSender;
        private readonly EmailSettings _emailSettings;

     
        public OrdersController( IWebHostEnvironment webHostEnvironment, IConfiguration configuration, IOrdersRepository ordersRepository, ICustomerDetailsRepository customerDetailsRepository, IRateRepository rateRepository, IPurityRepository purityRepository, IProductRepository productRepository, IMaterialCategoryRepository materialCategoryRepository, IProductTypeRepository productTypeRepository, IStaffRepository staffRepository, IEmailSender emailSender, IOptions<EmailSettings> emailSettings)
        {
            //    _unitOfWork = unitOfWork;
            _environment = webHostEnvironment;
            _configuration = configuration;
            _ordersRepository = ordersRepository;
            _customerDetailsRepository = customerDetailsRepository;
            _rateRepository = rateRepository;
            _purityRepository = purityRepository;
            _materialCategoryRepository = materialCategoryRepository;
            _productTypeRepository = productTypeRepository;
            _staffRepository = staffRepository;
            _productrepository = productRepository;
            _emailSender = emailSender;
            _emailSettings = emailSettings.Value;
          
        }


        [HttpGet("fetchAllOrders")]
        public async Task<IActionResult> fetchAllOrders()
        {
            var ordersdata = _ordersRepository.All().Include(x => x.tblProduct).Include(x => x.tblCustomerDetails).ToList();
            if (ordersdata != null)
            {
                return Ok(new { status = "Success", data = ordersdata });
            }
            return Ok(new { data = "No Data" });
        }

        [HttpGet("fetchAllRates")]
        public async Task<IActionResult> fetchAllRates()
        {
            var ratesdata = _rateRepository.All().ToList();
            if (ratesdata != null)
            {
                return Ok(new { status = "Success", data = ratesdata });
            }
            return Ok(new { data = "No Data" });
        }

        [HttpGet("fetchTodayRates")]
        public async Task<IActionResult> fetchTodayRates()
        {

            var ratesdata = _purityRepository.All().ToList().OrderByDescending(x => x.CreatedOn).FirstOrDefault();

            if (ratesdata != null)
            {
                return Ok(new { status = "Success", data = ratesdata });
            }
            return Ok(new { status = "Success", messege = "no data " });

        }
        [HttpGet("ratestodayallcategories")]
        public async Task<IActionResult> ratestodayallcategories()
        {

            var ratesdata = _rateRepository.All().Include(x => x.tblMaterialCategory).ToList().OrderByDescending(x => x.LastUpdated).Distinct();
            if (ratesdata != null)
            {
                return Ok(new { status = "success", data = ratesdata });

            }
            return BadRequest();
            //var category = _materialCategoryRepository.All().ToList();
            //if (category != null)
            //{

            //    return Ok(new { status = "Success", data = ratesdata, category });
            //}


            //  }

        }


        [HttpPost("ratestodaybycategory")]

        public async Task<IActionResult> ratestodayabycategoriesId(tblMaterialCategory model)
        {

            var ratesdata = _rateRepository.All().Include(x => x.tblMaterialCategory).Where(x => x.Category_id == model.Id).OrderByDescending(x => x.CreatedOn).FirstOrDefault();


            if (ratesdata != null)

            {
                return Ok(new { status = "Success", data = ratesdata });
            }
            return Ok(new { data = "No Data" });
        }

        [HttpPost("ratesallbycategoryId")]

        public async Task<IActionResult> ratesallbycategoryId(tblMaterialCategory model)
        {

            var ratesdata = _rateRepository.All().Include(x => x.tblMaterialCategory).Where(x => x.Category_id == model.Id).FirstOrDefault();


            if (ratesdata != null)

            {
                return Ok(new { status = "Success", data = ratesdata });
            }
            return Ok(new { data = "No Data" });
        }
        [HttpPost("AllOrdersByCustomerId")]
        public async Task<IActionResult> AllOrdersByCustomerId(tblOrder model)
        {
            var Ordersdata = _ordersRepository.All().Include(x => x.tblProduct).Include(x => x.tblCustomerDetails).Where(x => x.Customer_Id == model.Customer_Id).ToList();
            if (Ordersdata != null)
            {
                return Ok(new { status = "Success", data = Ordersdata });
            }
            return Ok(new { data = "No Data" });
        }

        [HttpPost("InsertOrders")]
        public async Task<IActionResult> InsertOrders(tblOrder model)
        {

            var emaildata = _customerDetailsRepository.All().Where(x => x.Id == model.Customer_Id).FirstOrDefault();
            if (ModelState.IsValid)
            {
                Random generator = new Random();
                int r = generator.Next(100000, 1000000);
                string orderseries="GH"+r.ToString();
                tblOrder order = new tblOrder();
                order.orderNumber = orderseries;
                order.Customer_Id = model.Customer_Id;
                order.Product_id = model.Product_id;
                order.Qty = model.Qty;
                order.Price = model.Price;
                order.PaymentMode = model.PaymentMode;
                order.Offer = model.Offer;
                order.GovtTax = model.GovtTax;
                order.ReceivedAmt = model.ReceivedAmt;
                order.OnlineStatus = "Active";
                order.OrderStatus = model.OrderStatus;
                await _ordersRepository.InsertAsync(order);

                // await sendEmail1pdf(order.Id,order.Product_id, order.Customer_Id, order.OrderStatus);
                //  await  sendEmail(order.Id, order.Product_id, order.Customer_Id, order.OrderStatus);
                // await sendEmail(order.Id,order.Product_id,order.Customer_Id,order.OrderStatus);
                return Ok(new { Status = "Success", data = order });
            }
            return BadRequest();
        }

        //private async Task sendEmail1pdf(int Id, int Productid, int customerid, string Status)
        //{

        //    string Message = "<p>Your Order  -" + Status + " Completed Sucessfully</p>";
        //    var orderdata = _ordersRepository.All().Where(x => x.Id == Id).FirstOrDefault();
        //    var user = _customerDetailsRepository.All().Where(x => x.Id == customerid).FirstOrDefault();
        //    var productdata = _productrepository.All().Where(x => x.Id == Productid).FirstOrDefault();
        //    var data = "";
        //    var imagePath = productdata.Images.Split(',');
        //    data = "https://jewellerywebapplications.blob.core.windows.net/images/" + imagePath[0];
        //    if (orderdata.OrderStatus == "Confirm")
        //    {
        //        var viewPath = Path.Combine(_environment.WebRootPath + "/Templates", "OrderConfirmation.html");
        //        var template = System.IO.File.ReadAllText(viewPath);
        //        template = template.Replace("XXXABCXXX", Message);
        //        template = template.Replace("XXXCallUrlXXX", "<p style=\"color:#ffffff\">For queries contact us</p>");
        //        template = template.Replace("XXXXNameXXX", user.FirstName + " " + user.LastName);
        //        template = template.Replace("XXXXcuraddressXXX", user.CurrAdd);
        //        template = template.Replace("XXXXemailXXX", user.Email);
        //        template = template.Replace("XXXXpincodeXXX", user.PinCode);
        //        template = template.Replace("XXXXimagesXXX", data);
        //        template = template.Replace("XXXXorderXXX", user.OrderId);
        //        template = template.Replace("XXXXorderdateXXX", user.CreatedOn.ToString("dd/MM/yyyy"));
        //        template = template.Replace("XXXXmobileXXX", user.Mobile);
        //        template = template.Replace("XXXXproductnameXXX", productdata.Product_Name);
        //        template = template.Replace("XXXXsizeXXX", productdata.Size);
        //        template = template.Replace("XXXXvalueXXX", user.OrderValue);
        //        template = template.Replace("XXXStatusXXX", user.OrderStatus);

        //        await _emailSender.SendEmailAsync(user.Email, "Order Confirmation", $"" + template + "");
        //    }
        //    else if (orderdata.OrderStatus == "Delivered")
        //    {
        //        var viewPath = Path.Combine(_environment.WebRootPath + "/Templates", "Invoice.html");
        //     //   var viewPath1 = Path.Combine(_environment.WebRootPath + "/Templates", "Invoice.html");
        //        var template = System.IO.File.ReadAllText(viewPath);
        //        var viewPath1 = Path.Combine(_environment.WebRootPath + "/Templates", "OrderConfirmation.html");
        //        var template1 = System.IO.File.ReadAllText(viewPath1);
        //        //    var template1 = System.IO.File.ReadAllText(viewPath1);
        //        template = template.Replace("XXXABCXXX", Message);

        //        template = template.Replace("XXXCallUrlXXX", "<p style=\"color:#ffffff\">For queries contact us</p>");
        //        template = template.Replace("XXXXNameXXX", user.FirstName + " " + user.LastName);
        //        template = template.Replace("XXXXcuraddressXXX", user.CurrAdd);
        //        template = template.Replace("XXXXemailXXX", user.Email);
        //        template = template.Replace("XXXXpincodeXXX", user.PinCode);
        //        template = template.Replace("XXXXimagesXXX", productdata.Images.TrimEnd(','));
        //        template = template.Replace("XXXXorderXXX", user.OrderId);
        //        template1 = template1.Replace("XXXXorderdateXXX", user.CreatedOn.ToString("dd/MM/yyyy"));
        //        template1 = template1.Replace("XXXXmobileXXX", user.Mobile);
        //        template1 = template1.Replace("XXXXproductnameXXX", productdata.Product_Name);
        //        template1 = template1.Replace("XXXXsizeXXX", productdata.Size);
        //        template1 = template1.Replace("XXXXvalueXXX", user.OrderValue);
        //        template1 = template1.Replace("XXXStatusXXX", user.OrderStatus);
        //        template = template.Replace("XXXXpincodeXXX", user.PinCode);
        //        template = template.Replace("XXXXimagesXXX", productdata.Images.TrimEnd(','));
        //        byte[] pdfBytes = ConvertHtmlToPdf(template);
        //        var attachment = new Attachment(new MemoryStream(pdfBytes), "pdf_attachment.pdf", "application/pdf");
        //        var message = new MailMessage("info@mkgharejewellers.com", user.Email, "PDF Attachment", "Please find the attached PDF.");
        //        message.Attachments.Add(attachment);
        //        SendEmailWithAttachment(pdfBytes, template1, orderdata.Id);
        //      //  await _emailSender.SendEmailAsync(user.Email, "Order Delivered", $"" + template1 + message+"");
        //    }
        //    else if (orderdata.OrderStatus == "Order is On the way")
        //    {
        //        var viewPath = Path.Combine(_environment.WebRootPath + "/Templates", "orderOnTheWay.html");
        //        var template = System.IO.File.ReadAllText(viewPath);
        //        template = template.Replace("XXXABCXXX", Message);
        //        template = template.Replace("XXXCallUrlXXX", "<p style=\"color:#ffffff\">For queries contact us</p>");
        //        template = template.Replace("XXXXNameXXX", user.FirstName + " " + user.LastName);
        //        template = template.Replace("XXXXcuraddressXXX", user.CurrAdd);
        //        template = template.Replace("XXXXemailXXX", user.Email);
        //        template = template.Replace("XXXXpincodeXXX", user.PinCode);
        //        template = template.Replace("XXXXimagesXXX", productdata.Images.TrimEnd(','));
        //        template = template.Replace("XXXXorderXXX", user.OrderId);
        //        template = template.Replace("XXXXorderdateXXX", user.CreatedOn.ToString("dd/MM/yyyy"));
        //        template = template.Replace("XXXXmobileXXX", user.Mobile);
        //        template = template.Replace("XXXXproductnameXXX", productdata.Product_Name);
        //        template = template.Replace("XXXXsizeXXX", productdata.Size);
        //        template = template.Replace("XXXXvalueXXX", user.OrderValue);
        //        template = template.Replace("XXXStatusXXX", user.OrderStatus);

        //        await _emailSender.SendEmailAsync(user.Email, "YOUR ORDER IS ON THE WAY!", $"" + template + "");
        //    }
        //}

        //public void SendEmailWithAttachment(byte[] attachmentBytes)
        //{
        //    using (var client = new SmtpClient("smtp.example.com", 587))
        //    {
        //        client.EnableSsl = true;
        //        client.UseDefaultCredentials = false;
        //        client.Credentials = new NetworkCredential("your-email@example.com", "your-password");

        //        var message = new MailMessage("sender@example.com", "recipient@example.com", "PDF Attachment", "Please find the attached PDF.");

        //        var attachment = new Attachment(new MemoryStream(attachmentBytes), "pdf_attachment.pdf", "application/pdf");
        //        message.Attachments.Add(attachment);

        //        client.Send(message);
        //    }
        //}

        [HttpPost("InsertRates")]
        public async Task<IActionResult> InsertRates(tblRates model)
        {
            if (ModelState.IsValid)
            {
                tblRates rate = new tblRates();
                var staff = _staffRepository.All().Select(x => x.Id).First();

                rate.Category_id = model.Category_id;
                rate.Category_Label = model.Category_Label;
                rate.Purity = model.Purity;
                rate.todaysrate = model.todaysrate;
                rate.Staff_id = staff;
                rate.OnlineStatus = "Active";
                await _rateRepository.InsertAsync(rate);
                return Ok(new { Status = "Success", data = rate });
            }
            return BadRequest();
        }
        [HttpPost("UpdateOrders")]
        public async Task<IActionResult> UpdateOrders(tblOrder model)
        {
            var orderdata = _ordersRepository.All().Where(x => x.Id == model.Id).FirstOrDefault();
            if (orderdata != null)
            {
                if (model.OrderStatus == "Paid")
                {
                    orderdata.Id = model.Id;
                    orderdata.OrderStatus = "Paid";
                    await _ordersRepository.UpdateAsync(orderdata, orderdata.Id);
                    await sendEmail(orderdata.Id, orderdata.Product_id, orderdata.Customer_Id);
                    return Ok(new { Status = "Success", data = orderdata });
                }
                else if (model.OrderStatus == "Delivered")
                {
                    orderdata.Id = model.Id;
                    orderdata.OnlineStatus = model.OnlineStatus;
                    orderdata.OrderStatus = "Delivered";
                    await _ordersRepository.UpdateAsync(orderdata, orderdata.Id);

                     await sendEmail(orderdata.Id, orderdata.Product_id, orderdata.Customer_Id);
                    return Ok(new { Status = "Success", data = orderdata });
                }
                else if (model.OrderStatus == "Payment Failed")
                {
                    orderdata.Id = model.Id;
                    orderdata.OnlineStatus = model.OnlineStatus;
                    orderdata.OrderStatus = "Payment Failed";
                    await _ordersRepository.UpdateAsync(orderdata, orderdata.Id);
                    await sendEmail(orderdata.Id, orderdata.Product_id, orderdata.Customer_Id);
                    return Ok(new { Status = "Success", data = orderdata });
                }
                else if(model.OrderStatus == "Order Placed")
                {
                    orderdata.Id = model.Id;
                    orderdata.OnlineStatus = model.OnlineStatus;
                    orderdata.OrderStatus = "Order Placed";
                    await _ordersRepository.UpdateAsync(orderdata, orderdata.Id);
                    await sendEmail(orderdata.Id, orderdata.Product_id, orderdata.Customer_Id);
                    return Ok(new { Status = "Success", data = orderdata });

                }

            }


            return BadRequest();
        }
        private async Task sendEmail(int Id, int Productid, int customerid)
        {
            //string Message = "Your Order  -" + Status + "  Sucessfully";
            var orderdata = _ordersRepository.All().Where(x => x.Id == Id).FirstOrDefault();
            var user = _customerDetailsRepository.All().Where(x => x.Id == customerid).FirstOrDefault();
            var productdata = _productrepository.All().Where(x => x.Id == Productid).FirstOrDefault();
            var puritydata = _purityRepository.All().Where(x => x.Id == productdata.PurityId).FirstOrDefault();
            decimal totalsaleamount;
            decimal netamount;
            decimal cgstamount;
            decimal totalstwt;
            decimal totalnetwt;
            decimal totaltax;
            decimal sgstamount;
            decimal Making_Percentage;
            decimal Making_per_gram;
            decimal Making_Fixed_Amt;
            decimal totalgrwt;
            decimal grossTotalRate;
            decimal makingchrg;
            decimal MRP;
            if (productdata.MRP != 0)
            {
                var cgst = 1.5;
                var sgst = 1.5;

                MRP = Convert.ToDecimal(productdata.MRP) * Convert.ToDecimal(orderdata.Qty);
                cgstamount = (Convert.ToDecimal(cgst) * MRP) / 100;
                sgstamount = (Convert.ToDecimal(sgst) * MRP) / 100;
                totaltax = cgstamount + sgstamount;
                totalsaleamount = MRP;
                totalnetwt = productdata.NetWt;
                var finalPrice = MRP + totaltax;
                grossTotalRate = totalsaleamount;
                netamount = finalPrice;
                totalgrwt = productdata.grosswt * orderdata.Qty;
                totalstwt = Convert.ToDecimal(productdata.StoneWeight) * orderdata.Qty;
                Making_Percentage = 0;
                Making_per_gram = 0;
                Making_Fixed_Amt = 0;
                makingchrg = 0;
            }
            else
            {
                var netGoldRate = (Convert.ToDecimal(productdata.NetWt) * Convert.ToDecimal(puritydata.TodaysRate)) / 10;
                var makingCharges1 = Convert.ToDecimal(productdata.NetWt) * Convert.ToDecimal(productdata.Making_per_gram);
                var makingCharges2 = (netGoldRate * Convert.ToDecimal(productdata.Making_Percentage)) / 100;
                var makingCharges3 = Convert.ToDecimal(productdata.Making_Fixed_Amt);
                var makingCharges4 = (Convert.ToDecimal(puritydata.TodaysRate) * Convert.ToDecimal(productdata.Making_Fixed_Wastage)) / 10;
                makingchrg = makingCharges1 + makingCharges2 + makingCharges3 + makingCharges4;
                var GST = 0.03;
                grossTotalRate = 1;

                grossTotalRate = netGoldRate + makingCharges1 + makingCharges2 + makingCharges3 + makingCharges4 + Convert.ToDecimal(productdata.StoneAmount);
                var GSTAdded = Convert.ToDecimal(GST) * grossTotalRate;
                var finalPrice = grossTotalRate + GSTAdded;

                //var productprice = (Convert.ToInt32(puritydata.TodaysRate) / 10) * productdata.NetWt;
                //var makingperc = productprice * Convert.ToInt32(productdata.Making_Percentage) / 100;
                //var total = productprice + makingperc;
                var cgst = 1.5;
                var sgst = 1.5;
                // var igst = 3;
                totalsaleamount = Convert.ToDecimal(grossTotalRate * orderdata.Qty);
                cgstamount = Convert.ToDecimal(cgst) * totalsaleamount / 100;
                sgstamount = Convert.ToDecimal(sgst) * totalsaleamount / 100;
                // var igstamount = Convert.ToDecimal(igst) * totalsaleamount / 100;
                totalnetwt = productdata.NetWt * orderdata.Qty;
                totalgrwt = productdata.grosswt * orderdata.Qty;
                totalstwt = Convert.ToDecimal(productdata.StoneWeight) * (Convert.ToDecimal(orderdata.Qty));

                totaltax = cgstamount + sgstamount;
                netamount = totalsaleamount + Convert.ToDecimal(totaltax);
                MRP = netamount;
            }
            var data = "";
            var imagePath = productdata.Images.Split(',');
            data = "https://jewellerywebapplications.blob.core.windows.net/images/" + imagePath[0];

            if (orderdata.OrderStatus == "Paid")
            {
                var viewPath = Path.Combine(_environment.WebRootPath + "/Templates", "OrderConfirmation.html");
                var template = System.IO.File.ReadAllText(viewPath);

                template = template.Replace("XXXCallUrlXXX", "<p style=\"color:#ffffff\">For queries contact us</p>");
                template = template.Replace("XXXXNameXXX", user.FirstName + " " + user.LastName);
                template = template.Replace("XXXXcuraddressXXX", user.PerAdd);
                template = template.Replace("XXXXemailXXX", user.Email);
                template = template.Replace("XXXXpincodeXXX", user.PinCode);
                template = template.Replace("XXXXquantityXXX", orderdata.Qty.ToString());
                template = template.Replace("XXXXsizeXXX", productdata.Size);
                template = template.Replace("XXXXimagesXXX", data);
                template = template.Replace("XXXXorderXXX", orderdata.orderNumber);
                template = template.Replace("XXXXorderdateXXX", orderdata.CreatedOn.ToString("dd/MM/yyyy"));
                template = template.Replace("XXXXmobileXXX", user.Mobile);
                template = template.Replace("XXXXproductnameXXX", productdata.Product_Name);
                template = template.Replace("XXXXitemcodeXXX", productdata.ItemCode);
                //  template = template.Replace("XXXXorderidXXX", orderdata.Id.ToString());
                template = template.Replace("XXXnetamtXXX", netamount.ToString());
                template = template.Replace("XXXStatusXXX", orderdata.OrderStatus);
                template = template.Replace("XXXpriceXXX", MRP.ToString());
                template = template.Replace("XXXqtyXXX", orderdata.Qty.ToString());
                await _emailSender.SendEmailAsync(user.Email, "Order Confirmation", $"" + template + "");
            }
            else if (orderdata.OrderStatus == "Delivered")
            {
                string connectionString = "DefaultEndpointsProtocol=https;AccountName=jewellerywebapplications;AccountKey=ZaTHTvbGMADLIQLKM4ujTLLDwoCMDRGIIfeVBdjPN+taeuEmhqXV51R1IoCdFgOwdMRQRaZ7Axgr+AStBPFF2g==;EndpointSuffix=core.windows.net";
                string containerName = "template";
                string blobName = "Invoice.html";


                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                Response<BlobDownloadInfo> downloadResponse = await blobClient.DownloadAsync();
                var template1 = await new StreamReader(downloadResponse.Value.Content).ReadToEndAsync();
                var viewPath = Path.Combine(_environment.WebRootPath + "/Templates", "OrderDelivered.html");
                //  var viewPath1 = Path.Combine(_environment.WebRootPath + "/Templates", "Invoice.html");
                var template = System.IO.File.ReadAllText(viewPath);
                //var viewPath1 = Path.Combine(_environment.WebRootPath + "/Templates", "Invoice.html");
                //  var viewPath1 = "https://jewellerywebapplications.blob.core.windows.net/template/Invoice.html";
                //   var template1 = System.IO.File.ReadAllText(viewPath1);
                //    var template1 = System.IO.File.ReadAllText(viewPath1);
                //  template = template.Replace("XXXABCXXX", Message);
                template = template.Replace("XXXCallUrlXXX", "<p style=\"color:#ffffff\">For queries contact us</p>");
                template = template.Replace("XXXXNameXXX", user.FirstName + " " + user.LastName);
                template = template.Replace("XXXXcuraddressXXX", user.CurrAdd);
                template = template.Replace("XXXXemailXXX", user.Email);
                template = template.Replace("XXXXpincodeXXX", user.PinCode);
                template = template.Replace("XXXXquantityXXX", productdata.Quantity.ToString());
                template = template.Replace("XXXXsizeXXX", productdata.Size);
                template = template.Replace("XXXXimagesXXX", data);
                template = template.Replace("XXXXorderXXX", orderdata.orderNumber.ToString());
                template = template.Replace("XXXXorderdateXXX", orderdata.CreatedOn.ToString("dd/MM/yyyy"));
                template = template.Replace("XXXXmobileXXX", user.Mobile);
                template = template.Replace("XXXXproductnameXXX", productdata.Product_Name);
                template = template.Replace("XXXXitemcodeXXX", productdata.ItemCode);
                template = template.Replace("XXXXorderidXXX", orderdata.Id.ToString());
                template = template.Replace("XXXStatusXXX", orderdata.OrderStatus);
                template = template.Replace("XXXordervalueXXX", orderdata.ReceivedAmt.ToString());
                template = template.Replace("XXXpriceXXX", MRP.ToString());
                template = template.Replace("XXXqtyXXX", orderdata.Qty.ToString());
                template = template.Replace("XXXnetamtXXX", netamount.ToString());
                template1 = template1.Replace("XXXXNameXXX", user.FirstName + " " + user.LastName);
                template1 = template1.Replace("XXXXcuraddressXXX", user.PerAdd);
                template1 = template1.Replace("XXXXemailXXX", user.Email);
                template1 = template1.Replace("XXXXpincodeXXX", user.PinCode);
                template1 = template1.Replace("XXXXNameXXX", user.FirstName + " " + user.LastName);
                template1 = template1.Replace("XXXXorderXXX", user.OrderId);
                template1 = template1.Replace("XXXXorderdateXXX", orderdata.CreatedOn.ToString("dd/MM/yyyy"));
                template1 = template1.Replace("XXXXmobileXXX", user.Mobile);
                template1 = template1.Replace("XXXXproductnameXXX", productdata.Product_Name);
                template1 = template1.Replace("XXXXhsncodeXXX", productdata.hsn_code);
                template1 = template1.Replace("XXXXqtyXXX", orderdata.Qty.ToString());
                template1 = template1.Replace("XXXXpurityXXX", productdata.purity.ToString());
                template1 = template1.Replace("XXXXgrwtXXX", productdata.grosswt.ToString());
                template1 = template1.Replace("XXXXstonewtXXX", productdata.StoneWeight.ToString());
                template1 = template1.Replace("XXXXnetwtXXX", productdata.NetWt.ToString());
                template1 = template1.Replace("XXXXmkpercXXX", makingchrg.ToString());
                template1 = template1.Replace("XXXXsizeXXX", productdata.Size);
                template1 = template1.Replace("XXXXrateXXX", puritydata.TodaysRate);
                template1 = template1.Replace("XXXXtotalamountXXX", totalsaleamount.ToString());
                template1 = template1.Replace("XXXXvalueXXX", user.OrderValue);
                template1 = template1.Replace("XXXStatusXXX", user.OrderStatus);
                template1 = template1.Replace("XXXnetamtXXX", netamount.ToString());
                template1 = template1.Replace("XXXcgstXXX", cgstamount.ToString());
                template1 = template1.Replace("XXXsgstXXX", sgstamount.ToString());
                template1 = template1.Replace("XXXtotaltaxXXX", totaltax.ToString());
                template1 = template1.Replace("XXXXpincodeXXX", user.PinCode);
                template1 = template1.Replace("XXXXtotalXXX", grossTotalRate.ToString());
                template1 = template1.Replace("XXXXinvoiceXXX", orderdata.Id.ToString());
                template1 = template1.Replace("XXXXtotalnetwtXXX", totalnetwt.ToString());
                template1 = template1.Replace("XXXXtotalgrwtXXX", totalgrwt.ToString());
                template1 = template1.Replace("XXXXtotalstwtXXX", totalstwt.ToString());
                //template1 = template1.Replace("XXXXinvoiceXXX", orderdata.Id.ToString());
                //try
                //{
                byte[] pdfBytes;
                var htmlContent = template1;
                using (MemoryStream stream = new MemoryStream())
                {
                    if (string.IsNullOrEmpty(htmlContent))
                    {
                        throw new Exception("HTML content is null or empty.");
                    }

                    ConverterProperties converterProperties = new ConverterProperties();
                    HtmlConverter.ConvertToPdf(htmlContent, stream, converterProperties);

                    pdfBytes = stream.ToArray();
                }
                var subject = "order delivered";
                var client = new SmtpClient(_emailSettings.MailServer)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_emailSettings.Sender, _emailSettings.Password),
                    Port = _emailSettings.MailPort,
                    EnableSsl = _emailSettings.SSL
                };
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("info@mkgharejewellers.com")
                };
                var attachment = new Attachment(new MemoryStream(pdfBytes), "Invoice.pdf", "application/pdf");
                mailMessage.Attachments.Add(attachment);
                mailMessage.To.Add(user.Email);
                mailMessage.Subject = subject;
                mailMessage.Body = template;
                mailMessage.IsBodyHtml = true;

                await client.SendMailAsync(mailMessage);
                // Email sent successfully
                //}
                //catch (Exception ex)
                //{

                //    return ex.Message;
                //}

            }
            else if (orderdata.OrderStatus == "Payment Failed")
            {
                var viewPath = Path.Combine(_environment.WebRootPath + "/Templates", "PaymentFailed.html");
                //   var viewPath1 = Path.Combine(_environment.WebRootPath + "/Templates", "Invoice.html");
                var template = System.IO.File.ReadAllText(viewPath);


                //    var template1 = System.IO.File.ReadAllText(viewPath1);
                //  template = template.Replace("XXXABCXXX", Message);

                template = template.Replace("XXXXNameXXX", user.FirstName + " " + user.LastName);
                template = template.Replace("XXXXcuraddressXXX", user.CurrAdd);
                template = template.Replace("XXXXemailXXX", user.Email);
                template = template.Replace("XXXXpincodeXXX", user.PinCode);
                template = template.Replace("XXXXqtyXXX", orderdata.Qty.ToString());
                template = template.Replace("XXXXsizeXXX", productdata.Size);
                template = template.Replace("XXXXimagesXXX", data);
                template = template.Replace("XXXXorderXXX", orderdata.orderNumber.ToString());
                template = template.Replace("XXXXorderdateXXX", user.CreatedOn.ToString("dd/MM/yyyy"));
                template = template.Replace("XXXXmobileXXX", user.Mobile);
                template = template.Replace("XXXXproductnameXXX", productdata.Product_Name);
                template = template.Replace("XXXXitemcodeXXX", productdata.ItemCode);
                template = template.Replace("XXXXorderidXXX", orderdata.Id.ToString());
                template = template.Replace("XXXStatusXXX", orderdata.OrderStatus);
                template = template.Replace("XXXordervalueXXX", orderdata.ReceivedAmt.ToString());
                template = template.Replace("XXXpriceXXX", MRP.ToString());

                await _emailSender.SendEmailAsync(user.Email, "Payment Failure - Order #" + orderdata.Id + "", $"" + template + "");
            }
            else if (orderdata.OrderStatus == "Order Placed")
            {
                var viewPath = Path.Combine(_environment.WebRootPath + "/Templates", "orderOnTheWay.html");
                //   var viewPath1 = Path.Combine(_environment.WebRootPath + "/Templates", "Invoice.html");
                var template = System.IO.File.ReadAllText(viewPath);


                //    var template1 = System.IO.File.ReadAllText(viewPath1);
                //  template = template.Replace("XXXABCXXX", Message);

                template = template.Replace("XXXXNameXXX", user.FirstName + " " + user.LastName);
                template = template.Replace("XXXXcuraddressXXX", user.CurrAdd);
                template = template.Replace("XXXXemailXXX", user.Email);
                template = template.Replace("XXXXpincodeXXX", user.PinCode);
                template = template.Replace("XXXXquantityXXX", productdata.Quantity.ToString());
                template = template.Replace("XXXXsizeXXX", productdata.Size);
                template = template.Replace("XXXXimagesXXX", data);
                template = template.Replace("XXXXorderXXX", orderdata.orderNumber.ToString());
                template = template.Replace("XXXXorderdateXXX", orderdata.CreatedOn.ToString("dd/MM/yyyy"));
                template = template.Replace("XXXXmobileXXX", user.Mobile);
                template = template.Replace("XXXXproductnameXXX", productdata.Product_Name);
                template = template.Replace("XXXXitemcodeXXX", productdata.ItemCode);
                template = template.Replace("XXXXorderidXXX", orderdata.Id.ToString());
                template = template.Replace("XXXStatusXXX", orderdata.OrderStatus);
                template = template.Replace("XXXordervalueXXX", orderdata.ReceivedAmt.ToString());
                template = template.Replace("XXXXtotalamountXXX", totalsaleamount.ToString());
                template = template.Replace("XXXXqtyXXX", orderdata.Qty.ToString());
                template = template.Replace("XXXpriceXXX", MRP.ToString());
                template = template.Replace("XXXnetamtXXX", netamount.ToString());
                await _emailSender.SendEmailAsync(user.Email, "Your Order Has Been Shipped!", $"" + template + "");
            }
            }
        [HttpPost("DownloadPDF")]
        public IActionResult DownloadPDF(tblOrder model)
        {
            var orderdata = _ordersRepository.All().Where(x => x.Id == model.Id).FirstOrDefault();
            var user = _customerDetailsRepository.All().Where(x => x.Id == orderdata.Customer_Id).FirstOrDefault();
            var productdata = _productrepository.All().Where(x => x.Id == orderdata.Product_id).FirstOrDefault();
            var puritydata = _purityRepository.All().Where(x => x.Id == productdata.PurityId).FirstOrDefault();
            decimal totalsaleamount;
            decimal netamount;
            decimal cgstamount;
            decimal totalstwt;
            decimal totalnetwt;
            decimal totaltax;
            decimal sgstamount;
            decimal Making_Percentage;
            decimal Making_per_gram;
            decimal Making_Fixed_Amt;
            decimal totalgrwt;
            decimal grossTotalRate;
            decimal makingchrg;
            decimal MRP;
            if (productdata.MRP != 0)
            {
                var cgst = 1.5;
                var sgst = 1.5;

                MRP = Convert.ToDecimal(productdata.MRP) * Convert.ToDecimal(orderdata.Qty);
                cgstamount = (Convert.ToDecimal(cgst) * MRP) / 100;
                sgstamount = (Convert.ToDecimal(sgst) * MRP) / 100;
                totaltax = cgstamount + sgstamount;
                totalsaleamount = MRP;
                totalnetwt = productdata.NetWt;
                var finalPrice = MRP + totaltax;
                grossTotalRate = totalsaleamount;
                netamount = finalPrice;
                totalgrwt = productdata.grosswt * orderdata.Qty;
                totalstwt = Convert.ToDecimal(productdata.StoneWeight) * orderdata.Qty;
                Making_Percentage = 0;
                Making_per_gram = 0;
                Making_Fixed_Amt = 0;
                makingchrg = 0;
            }
            else
            {
                var netGoldRate = (Convert.ToDecimal(productdata.NetWt) * Convert.ToDecimal(puritydata.TodaysRate)) / 10;
                var makingCharges1 = Convert.ToDecimal(productdata.NetWt) * Convert.ToDecimal(productdata.Making_per_gram);
                var makingCharges2 = (netGoldRate * Convert.ToDecimal(productdata.Making_Percentage)) / 100;
                var makingCharges3 = Convert.ToDecimal(productdata.Making_Fixed_Amt);
                var makingCharges4 = (Convert.ToDecimal(puritydata.TodaysRate) * Convert.ToDecimal(productdata.Making_Fixed_Wastage)) / 10;
                makingchrg = makingCharges1 + makingCharges2 + makingCharges3 + makingCharges4;
                var GST = 0.03;
                grossTotalRate = 1;

                grossTotalRate = netGoldRate + makingCharges1 + makingCharges2 + makingCharges3 + makingCharges4 + Convert.ToDecimal(productdata.StoneAmount);
                var GSTAdded = Convert.ToDecimal(GST) * grossTotalRate;
                var finalPrice = grossTotalRate + GSTAdded;

                //var productprice = (Convert.ToInt32(puritydata.TodaysRate) / 10) * productdata.NetWt;
                //var makingperc = productprice * Convert.ToInt32(productdata.Making_Percentage) / 100;
                //var total = productprice + makingperc;
                var cgst = 1.5;
                var sgst = 1.5;
                // var igst = 3;
                totalsaleamount = Convert.ToDecimal(grossTotalRate * orderdata.Qty);
                cgstamount = Convert.ToDecimal(cgst) * totalsaleamount / 100;
                sgstamount = Convert.ToDecimal(sgst) * totalsaleamount / 100;
                // var igstamount = Convert.ToDecimal(igst) * totalsaleamount / 100;
                totalnetwt = productdata.NetWt * orderdata.Qty;
                totalgrwt = productdata.grosswt * orderdata.Qty;
                totalstwt = Convert.ToDecimal(productdata.StoneWeight) * (Convert.ToDecimal(orderdata.Qty));

                totaltax = cgstamount + sgstamount;
                netamount = totalsaleamount + Convert.ToDecimal(totaltax);
                MRP = netamount;
            }
            var data = "";
            var imagePath = productdata.Images.Split(',');
            data = "https://jewellerywebapplications.blob.core.windows.net/images/" + imagePath[0];
            var viewPath = Path.Combine(_environment.WebRootPath + "/Templates", "Invoice.html");
            //  var viewPath1 = Path.Combine(_environment.WebRootPath + "/Templates", "Invoice.html");
            var template1 = System.IO.File.ReadAllText(viewPath);
            template1 = template1.Replace("XXXXNameXXX", user.FirstName + " " + user.LastName);
            template1 = template1.Replace("XXXXcuraddressXXX", user.PerAdd);
            template1 = template1.Replace("XXXXemailXXX", user.Email);
            template1 = template1.Replace("XXXXpincodeXXX", user.PinCode);
            template1 = template1.Replace("XXXXNameXXX", user.FirstName + " " + user.LastName);
            template1 = template1.Replace("XXXXorderXXX", user.OrderId);
            template1 = template1.Replace("XXXXorderdateXXX", user.LastUpdated.ToString("dd/MM/yyyy"));
            template1 = template1.Replace("XXXXmobileXXX", user.Mobile);
            template1 = template1.Replace("XXXXproductnameXXX", productdata.Product_Name);
            template1 = template1.Replace("XXXXhsncodeXXX", productdata.hsn_code);
            template1 = template1.Replace("XXXXqtyXXX", orderdata.Qty.ToString());
            template1 = template1.Replace("XXXXpurityXXX", productdata.purity.ToString());
            template1 = template1.Replace("XXXXgrwtXXX", productdata.grosswt.ToString());
            template1 = template1.Replace("XXXXstonewtXXX", productdata.StoneWeight.ToString());
            template1 = template1.Replace("XXXXnetwtXXX", productdata.NetWt.ToString());
            template1 = template1.Replace("XXXXmkpercXXX", makingchrg.ToString());
            template1 = template1.Replace("XXXXsizeXXX", productdata.Size);
            template1 = template1.Replace("XXXXrateXXX", puritydata.TodaysRate);
            template1 = template1.Replace("XXXXtotalamountXXX", totalsaleamount.ToString());
            template1 = template1.Replace("XXXXvalueXXX", user.OrderValue);
            template1 = template1.Replace("XXXStatusXXX", user.OrderStatus);
            template1 = template1.Replace("XXXnetamtXXX", netamount.ToString());
            template1 = template1.Replace("XXXcgstXXX", cgstamount.ToString());
            template1 = template1.Replace("XXXsgstXXX", sgstamount.ToString());
            template1 = template1.Replace("XXXtotaltaxXXX", totaltax.ToString());
            template1 = template1.Replace("XXXXpincodeXXX", user.PinCode);
            template1 = template1.Replace("XXXXtotalXXX", grossTotalRate.ToString());
            template1 = template1.Replace("XXXXinvoiceXXX", orderdata.Id.ToString());
            template1 = template1.Replace("XXXXtotalnetwtXXX", totalnetwt.ToString());
            template1 = template1.Replace("XXXXtotalgrwtXXX", totalgrwt.ToString());
            template1 = template1.Replace("XXXXtotalstwtXXX", totalstwt.ToString());
            byte[] pdfBytes;
            var htmlContent = template1;
            using (MemoryStream stream = new MemoryStream())
            {
                if (string.IsNullOrEmpty(htmlContent))
                {
                    throw new Exception("HTML content is null or empty.");
                }

                ConverterProperties converterProperties = new ConverterProperties();
           
                HtmlConverter.ConvertToPdf(htmlContent, stream, converterProperties);

                pdfBytes = stream.ToArray();
            }
        
            return File(pdfBytes, "application/pdf", "downloaded.pdf");
        }


        //public async Task<IActionResult> GeneratePdfFromHtml(string htmlContent)
        //{
        //    var document = new PdfDocument();
        //    var html = htmlContent;
        //    var converter = new BasicConverter(new(PdfTools()));
        //    var option = new HtmlToPdfDocument
        //    {
        //        GlobalSettings =
        //        {
        //            //ColorMode = ColorMode.Color,
        //            Orientation = Orientation.Landscape,
        //            PaperSize = PaperKind.A4,

        //        },
        //        Objects =
        //        {
        //            new ObjectSettings
        //            {
        //                PagesCount=true,
        //                HtmlContent=html,
        //                WebSettings={DefaultEncoding="Utf-8"}
        //            }
        //        }
        //    };

        //    string filename = "Invoice.pdf";
        //    return File( "application/pdf", filename);
        //}

        //public byte[] ConvertHtmlToPdf(string htmlContent)
        //{
        //// Create a PDF converter instance
        //PdfConverter pdfConverter = new PdfConverter();

        //// Set license key if required
        // pdfConverter.LicenseKey = "4W9+bn19bn5ue2B+bn1/YH98YHd3d3c=";

        //// Convert HTML content to PDF
        //byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(htmlContent);

        //return pdfBytes;
        //  }
        //private static byte[] BuildPdf(string html)
        //{
        //    return OpenHtmlToPdf.Pdf.From(html).Content();
        //}

        [HttpPost("UpdateRates")]
        public async Task<IActionResult> UpdateRates(tblRates model)
        {
            var ratedata = _rateRepository.All().Where(x => x.Id == model.Id).FirstOrDefault();
            if (ratedata != null)
            {
                //ratedata.Category_id = model.Category_id;
                //ratedata.Category_Label = model.Category_Label;
                //ratedata.Purity = model.Purity;
                //rate.todaysrate = model.todaysrate;

                if (model.Category_id != 0)
                {
                    ratedata.Category_id = model.Category_id;
                }

                else if (model.Category_Label != null)
                {
                    ratedata.Category_Label = model.Category_Label;
                }
                else if (model.Purity != null)
                {
                    ratedata.Purity = model.Purity;
                }

                else if (model.todaysrate != 0)
                {
                    ratedata.todaysrate = model.todaysrate;
                }
                else if (model.OnlineStatus != null)
                {
                    ratedata.OnlineStatus = "Active";
                }

                await _rateRepository.UpdateAsync(ratedata, ratedata.Id);

                return Ok(new { Status = "Success", data = ratedata });
            }
            return BadRequest();
        }



        [HttpPost("PaymentRequest")]
        public async Task<IActionResult> PaymentRequest(PaymentViewModel model)
        {


            var total = model.amount * 100;
            string uniqueId = Guid.NewGuid().ToString();
            int orderId = model.orderId;
            int customerId = Convert.ToInt32(model.id);
            var payload = new
            {
                merchantId = "GHAREONLINE",
                merchantTransactionId = uniqueId /*"MT7850590068188112"*/,
                merchantUserId = "MUID123",
                amount = total,

                callbackUrl = $"https://www.mkgharejewellers.com",
                redirectUrl = $"https://www.mkgharejewellers.com/paymentsuccesspage?orderId={orderId}&custId={customerId}",
                mobileNumber = model.mobile,
                paymentInstrument = new
                {
                    type = "PAY_PAGE"
                }
            };

            var saltKey = "cdca9558-eb96-46c7-bed3-f96abfdd044d";
            var saltIndex = 1;
            var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)));
            var xVerifyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(base64Payload + "/pg/v1/pay" + saltKey));
            var xVerify = BitConverter.ToString(xVerifyBytes).Replace("-", "").ToLower() + "###" + saltIndex;


            var requestPayload = new { request = base64Payload };
            var jsonRequest = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {

                // client.DefaultRequestHeaders.Add("Content-Type", "application/json");
                client.DefaultRequestHeaders.Add("X-VERIFY", xVerify);
                var response = await client.PostAsync("https://api.phonepe.com/apis/hermes/pg/v1/pay", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();



                if (response.IsSuccessStatusCode)
                {


                    // Handle the successful response from PhonePe API
                    return Ok(jsonResponse);
                }
                else
                {
                    // Handle the error response from PhonePe API
                    return Ok(new { messege = "An error occurred:", data = jsonResponse });
                }
            }
        }


        [HttpGet("PaymentStatus")]
        public async Task<IActionResult> PaymentStatus(PaymentStatusViewModel model)
        {

            var payload = new
            {
                merchantId = "GHAREONLINE",
                merchantTransactionId = model.merchantTransactionId,
                merchantUserId = "MUID123",
                amount = model.amount,
                callbackUrl = "https://www.mkgharejewellers.com/",
                mobileNumber = "9819662720",
                paymentInstrument = new
                {
                    type = "UPI"
                }
            };

            var saltKey = "cdca9558-eb96-46c7-bed3-f96abfdd044d";
            var saltIndex = 1;
            var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)));
            var xVerifyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(base64Payload + "/pg/v1/pay" + saltKey));
            var xVerify = BitConverter.ToString(xVerifyBytes).Replace("-", "").ToLower() + "###" + saltIndex;


            var requestPayload = new { request = base64Payload };
            var jsonRequest = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {

                // client.DefaultRequestHeaders.Add("Content-Type", "application/json");
                client.DefaultRequestHeaders.Add("X-VERIFY", xVerify);
                var response = await client.PostAsync("https://api.phonepe.com/apis/hermes/pg/v1/status/{merchantId}/{merchantTransactionId}", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();



                if (response.IsSuccessStatusCode)
                {
                    // Handle the successful response from PhonePe API
                    return Ok(jsonResponse);
                }
                else
                {
                    // Handle the error response from PhonePe API
                    return Ok(new { messege = "An error occurred:", data = jsonResponse });
                }
            }
        }


    }

}


