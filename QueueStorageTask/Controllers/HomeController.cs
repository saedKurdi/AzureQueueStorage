using Microsoft.AspNetCore.Mvc;
using QueueStorageTask.Models;
using QueueStorageTask.Services.Concrete;
using System.Diagnostics;

namespace QueueStorageTask.Controllers;

public class HomeController : Controller
{
    private readonly QueueService queueServiceDiscountCupone;
    private readonly QueueService queueServiceCount;
    private readonly IConfiguration configuration;
    public HomeController(IConfiguration configuration)
    {
        this.configuration = configuration;
        var connectionString = configuration.GetConnectionString("AzureStorage");
        queueServiceDiscountCupone = new QueueService(connectionString,"discount");
        queueServiceCount = new QueueService(connectionString,"counter");
    }

    public IActionResult Index()
    {
        var model = new IndexViewModel { Cupon = "Read to open coupone .", CuponReaden = false};
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ReadCupon()
    {
        // getting count of coupons : 
        var countQueueMsg = await queueServiceCount.ReceiveMessageAsync();
        var countMsg = countQueueMsg.Body.ToString();

        if(int.Parse(countMsg) == 0)
        {
            var model_ = new IndexViewModel { Cupon = "There is no cupon left in trendyol .", CuponReaden = false, CuponsLeft = int.Parse(countMsg) };
            return View("Index",model_);
        }

        // getting coupon :
        var couponQueueMessage = await queueServiceDiscountCupone.ReceiveMessageAsync();
        var couponMsg = couponQueueMessage.Body.ToString();

        // deleting last message from queue : 
        await queueServiceCount.DeleteMessageAsync(countQueueMsg.MessageId,countQueueMsg.PopReceipt);

        // adding new count message to queue : 
        var newCount = int.Parse(countMsg) - 1;
        await queueServiceCount.SendMessageAsync(newCount.ToString());

        // returning model for view :
        var model = new IndexViewModel { Cupon = couponMsg, CuponReaden = true,CuponsLeft =  newCount};
        return View("Index",model);
    }

    [HttpGet]
    public async Task<IActionResult> UnreadCupon()
    {
        // getting count of coupons : 
        var countQueueMessage = await queueServiceCount.ReceiveMessageAsync();
        var countMsg = countQueueMessage.Body.ToString();

        // deleting last message from queue : 
        await queueServiceCount.DeleteMessageAsync(countQueueMessage.MessageId, countQueueMessage.PopReceipt);

        // adding new count message to queue : 
        var newCount = int.Parse(countMsg) == 10 ? 9 : int.Parse(countMsg);
        await queueServiceCount.SendMessageAsync((newCount+1).ToString());

        // returning model for view :
        var model = new IndexViewModel { Cupon = "No coupon left now .", CuponReaden = false, CuponsLeft = newCount + 1};
        return View("Index", model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
