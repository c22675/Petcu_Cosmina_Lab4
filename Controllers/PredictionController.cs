using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using static Petcu_Cosmina_Lab4.MLModel;

namespace Petcu_Cosmina_Lab4.Controllers
{
    public class PredictionController : Controller
    {
        private readonly IWebHostEnvironment _env;
        public PredictionController(IWebHostEnvironment env) => _env = env;

            public IActionResult Price(ModelInput input)
            {
                // Load the model
                MLContext mlContext = new MLContext();
                // Create predection engine related to the loaded train model
                var modelPath = Path.Combine(_env.ContentRootPath, "MLModel.mlnet");
                ITransformer mlModel = mlContext.Model.Load(modelPath, out var modelInputSchema);
                var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput,
                ModelOutput>(mlModel);
                // Try model on sample data to predict fair price
                ModelOutput result = predEngine.Predict(input);
                ViewBag.Price = result.Score;
                return View(input);
            }
        }
}
