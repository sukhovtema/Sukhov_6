using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sukhov_6.Model;

namespace Sukhov_6.ViewModel
{
    public enum SerializeTypes { XML, JSON }

    public class FilmViewModel
    {
        #region ДАННЫЕ ДЛЯ ПРЕДСТАВЛЕНИЯ

        private const string FILE_FORMAT = "xml|*.xml|json|*.json";
        private readonly Dictionary<int, SerializeTypes> SEREALIZE_TYPE = new Dictionary<int, SerializeTypes>
        {
            { 1, SerializeTypes.XML },
            { 2, SerializeTypes.JSON },
        };

        public BaseCommand AddCommand { get; }
        public BaseCommand RemoveCommand { get; }
        public BaseCommand SaveCommand { get; }
        public BaseCommand OpenCommand { get; }

        public FilmModel SelectedFilm { get; set; }

        public ObservableCollection<FilmModel> Films { get; set; }

        #endregion

        #region КОМАНДЫ

        public FilmViewModel()
        {
            // начальные данные
            Films = new ObservableCollection<FilmModel>
            {
                new FilmModel { Name = "Спасатели Малибу", Year = 2017, Rating = 6, Genre = "Комедия" },
                new FilmModel { Name = "Начало", Year = 2010, Rating = 8, Genre = "Фантастика" }
            };

            // функция добавления
            AddCommand = new BaseCommand
            (
                obj =>
                {
                    var film = new FilmModel();
                    Films.Add(film);
                }
            );

            // функция удаления
            RemoveCommand = new BaseCommand
            (
                obj =>
                {
                    var film = (FilmModel)obj;
                    Films.Remove(film);
                }
            );

            // функция сохранения
            SaveCommand = new BaseCommand
            (
                obj =>
                {
                    var dialogWindow = new SaveFileDialog { InitialDirectory = 
                        Environment.CurrentDirectory, Filter = FILE_FORMAT };
                    if (dialogWindow.ShowDialog() != true) 
                        return;

                    var type = SEREALIZE_TYPE[dialogWindow.FilterIndex];
                    var serializedData = Serialize(type);
                    var fs = File.Create(dialogWindow.FileName);
                    var sw = new StreamWriter(fs);

                    sw.Write(serializedData);
                    sw.Close();
                },
                obj => Films.Count > 0
            );

            // функция загрузки
            OpenCommand = new BaseCommand
            (
                obj =>
                {
                    var dialogWindow = new OpenFileDialog { InitialDirectory = 
                        Environment.CurrentDirectory, Filter = FILE_FORMAT };
                    if (dialogWindow.ShowDialog() != true) 
                        return;

                    var type = SEREALIZE_TYPE[dialogWindow.FilterIndex];
                    var sw = new StreamReader(dialogWindow.FileName);
                    var data = sw.ReadToEnd();
                    var parsedData = (List<FilmModel>)Deserialize(type, data);

                    Films.Clear();
                    parsedData.ForEach(item => Films.Add(item));

                    sw.Close();
                },
                obj => Films.Count > 0
            );
        }

        private string Serialize(SerializeTypes type)
        {
            switch (type)
            {
                case SerializeTypes.XML:
                    return SerizalizeXML();
                case SerializeTypes.JSON:
                    return SerizalizeJSON();
                default:
                    return "";
            }
        }

        private static object Deserialize(SerializeTypes type, string data)
        {
            switch (type)
            {
                case SerializeTypes.XML:
                    return DeserizalizeXML(data);
                case SerializeTypes.JSON:
                    return DeserizalizeJSON(data);
                default:
                    return "";
            }
        }

        private string SerizalizeXML() =>
            new XElement(
                    "Films",
                    Films.Select(film => new XElement(
                        "Film",
                        new XElement("Name", film.Name),
                        new XElement("Genre", film.Genre),
                        new XElement("Year", film.Year),
                        new XElement("Rating", film.Rating)
            )))
            .ToString();

        private string SerizalizeJSON() =>
            JsonSerializer.Serialize(Films);

        private static object DeserizalizeXML(string data) =>
            XElement
                .Parse(data)
                .Elements()
                .Select(item => new FilmModel
                {
                    Name = (string)item.Element("Name"),
                    Genre = (string)item.Element("Genre"),
                    Year = (int)item.Element("Year"),
                    Rating = (int)item.Element("Rating"),
                }
        )
        .ToList();

        private static object DeserizalizeJSON(string data) =>
            JsonSerializer.Deserialize<List<FilmModel>>(data);

        #endregion
    }
}
