using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Accord.IO;
using MGBreakCaptcha2.ColorML;
using MGBreakCaptcha2.ImageEngine;
using MGBreakCaptcha2.MLEngine;

namespace MGBreakCaptcha2
{
    [Serializable]
    public class CaptchaDecoder
    {
        public Guid Id { get; }
        private Dictionary _dictionary;
        public IMLEngine Engine { get; private set; }
        [NonSerialized] private List<DataSet> _dataSetList;
        private int _normalizedHeight;
        private int _normalizedWidth;
        private const string MLEngineNamespace = "MGBreakCaptcha2.MLEngine";
        private const string MLEngineSuffix = "MLEngine";
        private const string ImageEngineNamespace = "MGBreakCaptcha2.ImageEngine";
        private const string ImageEngineSuffix = "_ImageEngine";
        private readonly string[] fileTypes = { "*.png", "*.jpeg", "*.jpg", "*.jfif" };

        public CaptchaDecoder()
        {
            this.Id = Guid.NewGuid();
            Log.Write(this.GetType(), $"Decoder created with ID {Id}");
        }

        public CaptchaDecoder(IMLEngine engine)
        {
            this.Id = Guid.NewGuid();
            this.Engine = engine;
            Log.Write(this.GetType(), $"Decoder created with Engine {Engine.GetType().Name} and ID {Id}");
        }

        public List<string> GetAllImplementedMLEngines()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsInterface && t.IsClass && MLEngineNamespace.Equals(t.Namespace) && t.Name.EndsWith(MLEngineSuffix))
                .Select(t => t.Name.Replace(MLEngineSuffix, string.Empty))
                .ToList();
        }

        public List<string> GetAllImplementedCourts()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsInterface && t.IsClass && ImageEngineNamespace.Equals(t.Namespace) && t.Name.EndsWith(ImageEngineSuffix))
                .Select(t => t.Name.Replace(ImageEngineSuffix, string.Empty))
                .ToList();
        }

        public List<string> GetAllImplementedColors()
        {
            return Enum.GetNames(typeof(ColorsEnum)).ToList();
        }

        public void LoadDataSet(string[] imagesPaths, string[] imagesCourts)
        {
            Log.Write(this.GetType(), "Loading data sets");

            if (!(imagesPaths?.Length > 0)
                || !(imagesCourts?.Length >= 0)
                || !Equals(imagesPaths?.Length, imagesCourts?.Length)
                || imagesPaths?.Count(p => !Directory.Exists(p)) > 0)
            {
                throw new SystemException("Error on data sets!");
            }

            _dataSetList = new List<DataSet>();

            for (int i = 0; i < imagesPaths.Length; i++)
            {
                Log.Write(this.GetType(), $"Loading data from '{imagesPaths[i]}'");
                DataSet dataSet = new DataSet()
                {
                    CourtName = imagesCourts[i].ToUpper(),
                    CourtDirectory = new DirectoryInfo(imagesPaths[i]),
                    CourtImageEngine = CreateCourtImageEngineObject(imagesCourts[i].ToUpper())
                };

                FileInfo[] files =
                    fileTypes
                        .SelectMany(t => dataSet.CourtDirectory.GetFiles(t, SearchOption.TopDirectoryOnly))
                        .ToArray();

                List<DataSetItem> dataSetItemList = new List<DataSetItem>();

                foreach (FileInfo file in files)
                {
                    Bitmap imageBitmap = new Bitmap(file.FullName);
                    Bitmap[] charactersBitmap = dataSet.CourtImageEngine.ExtractCharacters(imageBitmap);
                    char[] charactersLabelsChar = file.Name.Split('-')[0].ToCharArray();

                    dataSetItemList.Add(
                        new DataSetItem()
                        {
                            originalImage = imageBitmap,
                            originalLabel = file.Name,
                            charactersBitmap = charactersBitmap,
                            charactersLabelsChar = charactersLabelsChar
                        });
                }

                dataSet.Items = dataSetItemList;
                _dataSetList.Add(dataSet);
            }

            NormalizeAndConvertDataSet();
        }

        public double Learn()
        {
            (List<DataSet> learnDataSets, List<DataSet> scoreDataSets) = SplitDataSet(_dataSetList);

            double[][] learnData =
                learnDataSets
                    .SelectMany(ds => ds.Items, (ds, item) => new { ds, item })
                    .SelectMany(c => c.item.charactersDouble)
                    .ToArray();

            int[] learnLabel =
                learnDataSets
                    .SelectMany(ds => ds.Items, (ds, item) => new { ds, item })
                    .SelectMany(c => c.item.charactersLabelsInt)
                    .ToArray();

            double[][] testData =
                scoreDataSets
                    .SelectMany(ds => ds.Items, (ds, item) => new { ds, item })
                    .SelectMany(c => c.item.charactersDouble)
                    .ToArray();

            int[] testLabel =
                scoreDataSets
                    .SelectMany(ds => ds.Items, (ds, item) => new { ds, item })
                    .SelectMany(c => c.item.charactersLabelsInt)
                    .ToArray();

            if (Engine != null)
                return Engine.Learn(learnData, learnLabel, testData, testLabel);

            Log.Write(this.GetType(), "No ML engine defined, selecting automatically");

            List<Type> engineList = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsInterface && t.IsClass && MLEngineNamespace.Equals(t.Namespace) && t.Name.EndsWith(MLEngineSuffix))
                .ToList();

            List<(IMLEngine, double)> resultList = new List<(IMLEngine, double)>();

            Parallel.ForEach(engineList, e =>
            {
                IMLEngine engine = Activator.CreateInstance(e) as IMLEngine;
                double score = engine.Learn(learnData, learnLabel, testData, testLabel);
                resultList.Add((engine, score));
            });

            (IMLEngine, double) best = resultList.OrderByDescending(v => v.Item2).First();

            Log.Write(this.GetType(), $"{best.Item1.GetType().Name} selected with score {best.Item2}");
            Engine = best.Item1;

            return best.Item2;
        }

        public string Predict(string imagePath, string courtName, string color = null)
        {
            return Predict(new Bitmap(imagePath), courtName, color);
        }

        public string Predict(Bitmap imageBitmap, string courtName, string color = null)
        {
            IImageEngine imageEngine = CreateCourtImageEngineObject(courtName);

            ColorsEnum? colorEnum = null;
            if (
                !string.IsNullOrWhiteSpace(color)
                && Enum.GetNames(typeof(ColorsEnum)).Any(c => c.Equals(color, StringComparison.InvariantCultureIgnoreCase)))
            {
                Enum.TryParse(color, true, out ColorsEnum tmpColorEnum);
                colorEnum = tmpColorEnum;
            }

            Bitmap[] charactersBitmap = imageEngine.ExtractCharacters(imageBitmap, colorEnum);
            double[][] charactersDouble =
                charactersBitmap
                    .Select(ch => imageEngine.ConvertImageToDouble(new Bitmap(ch, _normalizedWidth, _normalizedHeight)))
                    .ToArray();

            int[] x = Engine.Predict(charactersDouble);

            return string.Concat(DictionaryTools.IntToChar(_dictionary, x));
        }

        private void NormalizeAndConvertDataSet()
        {
            Log.Write(this.GetType(), "Normalizing data sets");

            List<DataSet> newDataSets = new List<DataSet>();

            List<Bitmap> allCharactersBitmap =
                _dataSetList
                    .SelectMany(ds => ds.Items, (ds, item) => new { ds, item })
                    .SelectMany(c => c.item.charactersBitmap)
                    .ToList();


            _normalizedHeight = allCharactersBitmap.Max(c => c.Height);
            _normalizedWidth = allCharactersBitmap.Max(c => c.Width);

            char[] labels =
                _dataSetList
                    .SelectMany(ds => ds.Items, (ds, item) => new { ds, item })
                    .SelectMany(c => c.item.charactersLabelsChar)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToArray();

            _dictionary = DictionaryTools.CreateDictionaries(labels);

            foreach (DataSet dataSet in _dataSetList)
            {
                Parallel.ForEach(dataSet.Items, dataSetItem =>
                {
                    dataSetItem.charactersLabelsInt =
                        DictionaryTools.CharToInt(_dictionary, dataSetItem.charactersLabelsChar);
                    dataSetItem.charactersBitmap =
                        dataSetItem
                            .charactersBitmap
                            .Select(ch => new Bitmap(ch, _normalizedWidth, _normalizedHeight))
                            .ToArray();
                    dataSetItem.charactersDouble =
                        dataSetItem
                            .charactersBitmap
                            .Select(ch =>
                                dataSet
                                    .CourtImageEngine
                                    .ConvertImageToDouble(new Bitmap(ch, _normalizedWidth, _normalizedHeight)))
                            .ToArray();
                });
            }
        }

        private (List<DataSet>, List<DataSet>) SplitDataSet(List<DataSet> dataSets, double division = 0.8)
        {
            Log.Write(this.GetType(), $"Splitting data sets");

            if (division <= 0 || division >= 1)
            {
                return (dataSets, dataSets);
            }

            Random random = new Random();

            List<DataSet> copyOriginalDataSets = dataSets.DeepClone();
            List<DataSet> learnDataSets = new List<DataSet>();
            List<DataSet> testDataSets = new List<DataSet>();
            
            Parallel.ForEach(copyOriginalDataSets, dataSet =>
            {
                List<DataSetItem> originalDataSetItems = dataSet.Items;

                int nItems = Convert.ToInt32(originalDataSetItems.Count * division);

                List<DataSetItem> learnDataSetItems = originalDataSetItems.DeepClone();
                List<DataSetItem> testDataSetItems = new List<DataSetItem>();

                List<int> indexes = new List<int>();
                int counter = 0;
                while (counter < originalDataSetItems.Count - nItems)
                {
                    int randInt = random.Next(originalDataSetItems.Count - 1);
                    if (!indexes.Exists(num => num == randInt))
                    {
                        indexes.Add(randInt);
                        counter++;
                    }
                }

                indexes = indexes.OrderByDescending(index => index).ToList();

                indexes.ForEach(learnDataSetItems.RemoveAt);
                indexes.ForEach(index => testDataSetItems.Add(originalDataSetItems.ElementAt(index)));

                dataSet.Items = null;

                DataSet learnDataSet = dataSet.DeepClone();
                learnDataSet.Items = learnDataSetItems;
                learnDataSets.Add(learnDataSet);

                DataSet testDataSet = dataSet.DeepClone();
                testDataSet.Items = testDataSetItems;
                testDataSets.Add(testDataSet);
            });

            return (learnDataSets, testDataSets);
        }

        private IImageEngine CreateCourtImageEngineObject(string court)
        {
            Type type = Type.GetType($"{ImageEngineNamespace}.{court.ToUpper()}{ImageEngineSuffix}");
            IImageEngine courtImageEngine = Activator.CreateInstance(type) as IImageEngine;
            return courtImageEngine;
        }
    }
}