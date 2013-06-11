using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Controls.Pivot;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Resources;

using Kent.Boogaart.KBCsv;

namespace TmiPivot
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();

            ObservableCollection<TmiImage> tmiImageList = new ObservableCollection<TmiImage>();
            try
            {
                StreamResourceInfo sri = null;
                try
                {
                    Uri csvUri = new Uri("TmiPivotMetadata.csv", UriKind.Relative);
                    sri = Application.GetResourceStream(csvUri);
                }
                catch (Exception exp2)
                {
                    System.Diagnostics.Debug.WriteLine("### Exception while getting resource stream: {0}", exp2.ToString());
                }

                using (CsvReader reader = new CsvReader(sri.Stream))
                {
                    reader.ValueDelimiter = '@';
                    HeaderRecord head = reader.ReadHeaderRecord();

                    //int recCount = 0;
                    while (reader.HasMoreRecords)// && recCount < 620)
                    {
                        DataRecord rec = reader.ReadDataRecord();
                        tmiImageList.Add(createFromRecord(rec));
                        //dumpRecord(rec);
                        //recCount++;
                    }

                    reader.Close();
                }

            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine("### Exception while reading csv file: {0}", exp.ToString());
            }

            MyPV.ItemsSource = tmiImageList;
        }

        private void GetCommands(object sender, PivotViewerCommandsRequestedEventArgs e)
	    {
            Uri uri = (e.Item as TmiImage).ImageBigPath;
            e.Commands.Add(new AdornerCommand(uri));
	    }

        private void MyPV_SelectionChanged(object sender, EventArgs e)
        {
            TmiImage.LoadBigImage(MyPV.SelectedItem as TmiImage);
        }

        public void dumpRecord(DataRecord rec)
        {
            System.Diagnostics.Debug.WriteLine("Dumping DataRecord");
            System.Diagnostics.Debug.WriteLine("nodename: {0}\nidname: {1}\nimagename: {2}\nfilename: {3}\nfilenamemedium: {4}\nfilenamethumb: {5}",
                rec["nodename"], rec["idname"], rec["imagename"], rec["filename"], rec["filenamemedium"], rec["filenamethumb"]);
            System.Diagnostics.Debug.WriteLine("lakecode: {0}\nlakename: {1}\nmagnification: {2}\nnotes: {3}\nsection: {4}\nsitehole: {5}",
                rec["lakecode"], rec["lakename"], rec["magnification"], rec["notes"], rec["section"], rec["sitehole"]);
            System.Diagnostics.Debug.WriteLine("submittedby: {0}\nyear: {1}\ntaxon: {2}\nimageuitags: {3}\ndescription: {4}\ndistinguishingfeatures: {5}",
                rec["submittedby"], rec["year"], rec["taxon"], rec["imageuitags"], rec["description"], rec["distinguishingfeatures"]);
            System.Diagnostics.Debug.WriteLine("nodeuitags: {0}\nlighttypename: {1}", rec["nodeuitags"], rec["lighttypename"]);
        }

        private TmiImage createFromRecord(DataRecord rec)
        {
            return new TmiImage(rec["nodename"], rec["idname"], rec["imagename"], rec["filename"], rec["filenamemedium"], rec["filenamethumb"], rec["lakecode"], rec["lakename"],
                rec["magnification"], rec["notes"], rec["section"], rec["sitehole"], rec["submittedby"], rec["year"], rec["taxon"], rec["imageuitags"], rec["description"],
                rec["distinguishingfeatures"], rec["nodeuitags"], rec["lighttypename"]);
        }
    }

    public class TmiImage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string NodeName { get; set; }
        public string IdName { get; set; }
        public string ImageName { get; set; }
        public string FilenameBig { get; set; }
        public string FilenameMedium { get; set; }
        public string FilenameThumb { get; set; }
        public string ExpeditionCode { get; set; }
        public string LakeCode { get; set; }
        public string LakeName { get; set; }
        public string Magnification { get; set; }
        public string Notes { get; set; }
        public string Section { get; set; }
        public string SiteHole { get; set; }
        public string SubmittedBy { get; set; }
        public string Year { get; set; }
        public string Taxon { get; set; }
        public List<string> ImageUiTags { get; set; }
        public string ImageDescription { get; set; } // naming "Description" led to this text always showing in the title of the properties pane
        public List<string> DistinguishingFeatures { get; set; }
        public List<string> NodeUiTags { get; set; }
        public string LightTypeName { get; set; }

        public Uri ImageBigPath { get; set; }
        public string ImageMediumPath { get; set; }

        public BitmapImage ImageThumb { get; set; }
        public BitmapImage imageMedium;
        public BitmapImage ImageMedium
        {
            get { return imageMedium; }
            set
            {
                if (imageMedium == null || !imageMedium.Equals(value as BitmapImage))
                {
                    imageMedium = value;
                    if (!ImageMediumLoaded && PropertyChanged != null)
                    {
                        ImageMediumLoaded = true;
                        PropertyChanged(this, new PropertyChangedEventArgs("ImageMedium"));
                        PropertyChanged(this, new PropertyChangedEventArgs("ImageMediumLoaded"));
                    }
                }
            }
        }

        // members strictly for display purposes
        public SolidColorBrush HeadColor { get; set; }
        public string HeadTitle { get; set; }
        public string TagBlob { get; set; }
        public string TaxonItalic { get; set; }
        public string TaxonPlain { get; set; }

        // To trigger redraw of a trading card once its medium-res image is downloaded, a pivot property (anything in
        // <sdk:PivotViewer.PivotProperties>) needs to be changed. Added this semi-dummy property to force redraw
        // without touching actual image metadata.
        public bool ImageMediumLoaded { get; set; }

        public TagStringList parseTags(string tagString)
        {
            TagStringList tagList = null;
            if (tagString.Length > 0 && tagString != "\\N")
            {
                tagString = tagString.ToLower();
                char[] separator = { ' ' };
                tagList = new TagStringList();
                foreach (string s in tagString.Split(separator))
                {
                    tagList.Add(s);
                }
            }
            else
            {
                tagList = new TagStringList();
            }
            return tagList;
        }

        public TmiImage(string nodename, string idname, string imagename, string filename, string filenamemedium, string filenamethumb, string lakecode, string lakename, string magnification,
            string notes, string section, string sitehole, string submittedby, string year, string taxon, string imageuitags, string description, string distinguishingfeatures, string nodeuitags, string lighttypename)
        {
            NodeName = nodename;
            IdName = idname;
            ImageName = imagename;
            FilenameBig = filename;
            FilenameMedium = filenamemedium;
            FilenameThumb = filenamethumb;

            LakeCode = handleNullValue(lakecode);
            LakeName = handleNullValue(lakename);
            Magnification = handleNullValue(magnification);
            Notes = handleNullValue(notes);
            Section = handleNullValue(section);
            SiteHole = handleNullValue(sitehole);
            SubmittedBy = handleNullValue(submittedby);
            Year = handleNullValue(year);
            Taxon = handleNullValue(taxon);
            ImageUiTags = parseTags(imageuitags);
            ImageDescription = handleNullValue(description);
            DistinguishingFeatures = parseTags(distinguishingfeatures);
            NodeUiTags = parseTags(nodeuitags);
            LightTypeName = lighttypename;

            ImageMediumLoaded = false;

            if (IdName == "Plant")
                HeadColor = new SolidColorBrush(Colors.Green);
            else if (IdName == "Mineral")
                HeadColor = new SolidColorBrush(Color.FromArgb(255, 158, 16, 16)); // dark red
            else if (IdName == "Arthropod")
                HeadColor = new SolidColorBrush(Colors.Purple);
            else if (IdName == "Algae")
                HeadColor = new SolidColorBrush(Color.FromArgb(255, 60, 179, 113)); // "medium sea green"
            else if (IdName == "Invertebrate")
                HeadColor = new SolidColorBrush(Colors.Blue);
            else if (IdName == "Fish")
                HeadColor = new SolidColorBrush(Color.FromArgb(255, 102, 0, 0)); // dark brown
            else if (IdName == "Contaminant")
                HeadColor = new SolidColorBrush(Colors.Orange);
            else if (IdName == "Lithofacies")
                HeadColor = new SolidColorBrush(Colors.DarkGray);
            HeadTitle = IdName + " :: " + NodeName;

            string result = String.Empty;
            if (DistinguishingFeatures.Count > 0)
            {
                string distFeats = String.Empty;
                foreach (string df in DistinguishingFeatures)
                {
                    distFeats += df + " ";
                }
                result += distFeats;
            }
            if (ImageUiTags.Count > 0)
            {
                string iut = String.Empty;
                foreach (string imageTag in ImageUiTags)
                {
                    iut += imageTag + " ";
                }
                result += iut;
            }
            if (NodeUiTags.Count > 0)
            {
                string nut = String.Empty;
                foreach (string nodeTag in NodeUiTags)
                {
                    nut += nodeTag + " ";
                }
                result += nut;
            }
            TagBlob = (result.Length > 0) ? result : "[no info]";

            TaxonItalic = String.Empty;
            TaxonPlain = String.Empty;

            char[] delimiter = { ' ' };
            string[] taxonTokens = Taxon.Split(delimiter, StringSplitOptions.None);
            int curTok = 0;
            foreach (string tok in taxonTokens)
            {
                if (curTok < 2)
                {
                    if (tok.ToLower().Equals("sp."))
                        TaxonPlain += " " + tok;
                    else
                        TaxonItalic += " " + tok;
                }
                else
                    TaxonPlain += " " + tok;
                curTok++;
            }

            var loweridname = IdName.ToLower(new CultureInfo("en-US")).Replace(" ", string.Empty);
            var lowernodename = NodeName.Replace(" ", string.Empty);

            try
            {
                // thumbnails are included in application (.XAP, which is just a renamed ZIP archive) package
                string thumbPath = "components/" + loweridname + "/" + lowernodename + "/" + FilenameThumb;
                StreamResourceInfo thumbSri = Application.GetResourceStream(new Uri(thumbPath, UriKind.Relative));
                ImageThumb = new BitmapImage();
                ImageThumb.SetSource(thumbSri.Stream);

                // initially set ImageMedium source to ImageThumb - we download real ImageMedium on the fly
                ImageMedium = ImageThumb;
                ImageMediumPath = "http://www.beerolf.com/components/" + loweridname + "/" + lowernodename + "/" + FilenameMedium;
                ImageBigPath = new Uri("http://www.beerolf.com/components/" + loweridname + "/" + lowernodename + "/" + FilenameBig, UriKind.Absolute);
            }
            catch (Exception foo)
            {
                System.Diagnostics.Debug.WriteLine("### Exception while reading {0}: {1}", FilenameThumb, foo.ToString());
            }
        }

        public static void LoadBigImage(TmiImage ti)
        {
            BitmapImage bi = new BitmapImage();
            bi.UriSource = new Uri(ti.ImageMediumPath, UriKind.Absolute);
            ti.ImageMedium = bi;
        }

        private string handleNullValue(string rec)
        {
            return (rec.Length > 0 && rec != "\\N") ? rec : "";
        }
    }

    public class TagStringList : List<string>, IComparable
    {
        public int CompareTo(object obj)
        {
            if (this.Count > 0 && (obj as TagStringList).Count == 0)
                return -1; // this instance precedes the empty one
            else if (this.Count == 0 && (obj as TagStringList).Count > 0)
                return 1;  // obj precedes this empty instance
            else if (this.Count > 0 && (obj as TagStringList).Count > 0)
                return this[0].CompareTo((obj as TagStringList)[0]);
            else
                return 0;
        }
    }

    public class AdornerCommand : IPivotViewerUICommand
    {
        private readonly Uri uri;

        public AdornerCommand(Uri uri)
        {
            this.uri = uri;
        }

        public string DisplayName
        {
            get { return null; }
        }

        public Uri Icon
        {
            get { return new Uri("http://www.beerolf.com/img/magglass.png", UriKind.Absolute); }
        }

        public object ToolTip
        {
            get { return "Click to open high-resolution image in a new tab"; }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            HtmlPage.Window.Navigate(uri, "_blank");
        }
    }
}
