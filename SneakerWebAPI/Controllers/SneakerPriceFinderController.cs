using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SneakerWebAPI;
using SneakerWebAPI.Data;
using HtmlAgilityPack;
using Microsoft.Identity.Client;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Humanizer;
using System.Net.Http;
using SneakerWebAPI.Services.SneakerService;

namespace SneakerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SneakerPriceFinderController : ControllerBase
    {

        private readonly HttpClient _httpclient;
        private readonly DataContext _context;
        private readonly ISneakerService _sneakerservice;
        public SneakerPriceFinderController(DataContext context,ISneakerService sneaker)
        {
            _httpclient = new HttpClient();
            _httpclient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            _context = context;
            _sneakerservice = sneaker;
        }

        [HttpGet]
        public async Task<ActionResult<List<Sneaker>>> GetSneakerPrice()
        {
            var shoes = await _context.Sneakers.ToListAsync();
            if ((shoes == null))
            {
                return BadRequest("Sneaker was not Found");
            }
            foreach (var shoe1 in shoes)
            {

                string size = shoe1.Size.ToString();
                string source = shoe1.ResellURL;
                float price = _sneakerservice.GetPrice(size, source);
                shoe1.ResellPrice = price;
            }
            try
            {
                
            }
            catch (Exception Error)
            {
                Console.WriteLine(Error.Message);
            }
            return Ok(shoes);
        }
    



        [HttpPut]
        public async Task<ActionResult<List<Sneaker>>> UpdateSneaker(int id)
        {
            
          
            var shoe1 = await _context.Sneakers.FindAsync(id);
            if ((shoe1 == null))
            {
                return BadRequest("Sneaker was not Found");
            }
            string size = shoe1.Size.ToString();
            string source = shoe1.ResellURL;
            float price = _sneakerservice.GetPrice(size,source);
            shoe1.ResellPrice = price;
            await _context.SaveChangesAsync();
            return Ok();
        }

    //    [HttpPut]
    //    public async Task<ActionResult<List<Sneaker>>> UpdateAlSneakers()
    //    {


    //        var shoe1 = await _context.Sneakers.ToListAsync();
    //        if ((shoe1 == null))
    //        {
    //            return BadRequest("Sneaker was not Found");
    //        }
    //        foreach (var shoe in shoe1)
    //        {
    //            string size = shoe.Size.ToString();
    //            string source = shoe.ResellURL;
    //            float price = _sneakerservice.GetPrice(size, source);
    //            shoe.ResellPrice = price;
    //        }
    //        await _context.SaveChangesAsync();
    //        return Ok(shoe1);
    //    }

  }
    }

