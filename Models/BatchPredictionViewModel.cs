using static Petcu_Cosmina_Lab4.MLModel;

namespace Petcu_Cosmina_Lab4.Models
{
    public class BatchPredictionViewModel
    {
        public IFormFile? File { get; set; }
        // Rezultatele (predicțiile) pentru fiecare rând din fișier
        public List<ModelOutput>? Predictions { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
