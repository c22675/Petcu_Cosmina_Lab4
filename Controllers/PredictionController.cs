using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Petcu_Cosmina_Lab4.Models;
using Petcu_Cosmina_Lab4.Models.Data;
using static Petcu_Cosmina_Lab4.MLModel;

namespace Petcu_Cosmina_Lab4.Controllers
{
    public class PredictionController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context;
        public PredictionController(IWebHostEnvironment env, AppDbContext context)
        {
            _env = env;
            _context = context;
        }

        [HttpGet]
        public IActionResult Price()
        {
            return View(new ModelInput { Payment_type = "CSH" });
        }

        public async Task<IActionResult> Price(ModelInput input)
        {
            MLContext mlContext = new MLContext();
            var modelPath = Path.Combine(_env.ContentRootPath, "MLModel.mlnet");
            ITransformer mlModel = mlContext.Model.Load(modelPath, out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput,
            ModelOutput>(mlModel);
            ModelOutput result = predEngine.Predict(input);
            ViewBag.Price = result.Score;
          

            var history = new PredictionHistory
            {
                PassengerCount = input.Passenger_count,
                TripTimeInSecs = input.Trip_time_in_secs,
                TripDistance = input.Trip_distance,
                PaymentType = input.Payment_type,
                PredictedPrice = result.Score,
                CreatedAt = DateTime.Now
            };

            if (history != null)
            {
                _context.PredictionHistories.Add(history);
                await _context.SaveChangesAsync();
            }

            return View(input);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var history = await _context.PredictionHistories
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
            return View(history);
        }


        [HttpGet]
        public async Task<IActionResult> Dashboard(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.PredictionHistories.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(p => p.CreatedAt.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(p => p.CreatedAt.Date <= toDate.Value.Date);

            var totalPredictions = await query.CountAsync();

            var paymentTypeStats = await query
                .GroupBy(p => p.PaymentType)
                .Select(g => new PaymentTypeStat
                {
                    PaymentType = g.Key,
                    AveragePrice = g.Average(x => x.PredictedPrice),
                    Count = g.Count()
                })
                .ToListAsync();

            var allPredictions = await query
                .Select(p => p.PredictedPrice)
                .ToListAsync();

            var buckets = new List<PriceBucketStat>
            {
                new PriceBucketStat { Label = "0 - 10" },
                new PriceBucketStat { Label = "10 - 20" },
                new PriceBucketStat { Label = "20 - 30" },
                new PriceBucketStat { Label = "30 - 50" },
                new PriceBucketStat { Label = "> 50" }
            };

            foreach (var price in allPredictions)
            {
                if (price < 10) buckets[0].Count++;
                else if (price < 20) buckets[1].Count++;
                else if (price < 30) buckets[2].Count++;
                else if (price < 50) buckets[3].Count++;
                else buckets[4].Count++;
            }

            var vm = new DashboardViewModel
            {
                TotalPredictions = totalPredictions,
                PaymentTypeStats = paymentTypeStats,
                PriceBuckets = buckets,
                FromDate = fromDate,
                ToDate = toDate
            };

            return View(vm);
        }
    }
}