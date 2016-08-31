using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using POGOProtos.Map.Fort;
using PokemonGo.RocketAPI.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokemonGo.RocketAPI.Console
{
    public partial class Map : Form
    {
        static GMapRoute currentRoute = new GMapRoute("PlayerCurrentRoute");
        static List<FortData> currentForts = new List<FortData>();

        private delegate void FortDataCollectionMethod(IEnumerable<FortData> pokemondata);

        public Map()
        {
            Logic.Logging.Logger.Events.FortsChangedEvent += HandleFortsChangedEvent;
            Logic.Logging.Logger.Events.PlayerPositionChangedEvent += Events_PlayerPositionChangedEvent;


            InitializeComponent();
            gmap.MapProvider = GMap.NET.MapProviders.BingMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            gmap.Zoom = 15;
            // gmap.SetPositionByKeywords("London, England");
            gmap.Position = new PointLatLng(51.515546, -0.117404);

            gmap.MinZoom = 0;
            gmap.MaxZoom = 24;
            gmap.Zoom = 16;
            gmap.MarkersEnabled = true;

            GMapOverlay markersOverlay = new GMapOverlay("markers");
            gmap.Overlays.Add(markersOverlay);
            GMapOverlay routesOverlay = new GMapOverlay("routes");
            gmap.Overlays.Add(routesOverlay);
            routesOverlay.Routes.Add(currentRoute);
        }

        private void Events_PlayerPositionChangedEvent(object sender, Logic.PointLatLngEventArgs e)
        {
            if (gmap.InvokeRequired)
            {
                gmap.Invoke((MethodInvoker)(() => { UpdateMapPoint(e.LatLongPoint); }));
            }
            else
            {
                UpdateMapPoint(e.LatLongPoint);
            }
        }

        private void UpdateMapPoint(PointLatLng latlongPoint)
        {
            currentRoute.Points.Add(latlongPoint);

            var routeOverlay = gmap.Overlays.FirstOrDefault(x => x.Id == "routes");

            routeOverlay.Markers.Clear();
            GMarkerGoogle thisMarker = new GMarkerGoogle(latlongPoint, GMarkerGoogleType.red);
            routeOverlay.Markers.Add(thisMarker);
            
            //gmap.Refresh();
            //this.Refresh();
        }

        private void HandleFortsChangedEvent(object sender, Logic.FortDataCollectionEventArgs e)
        {
            LoadForts(e.FortCollection);
        }


        public void LoadForts(IEnumerable<FortData> forts)
        {
            double? maxLatitude = null;
            double? minLatitude = null;
            double? maxLongitude = null;
            double? minLongitude = null;
            if (this.InvokeRequired)
            {

                this.Invoke(
                    new FortDataCollectionMethod(LoadForts), // the method to call back on
                    new object[] { forts });
            }
            else
            {
                var markersOverlay = gmap.Overlays.FirstOrDefault(x => x.Id == "markers");
                if (markersOverlay == null)
                {
                    throw new Exception("Markers overlay not found");
                }

                long unixCurrentTime = DateTime.UtcNow.ToUnixTime();
                foreach (var fort in forts)
                {
                    var listedFort = currentForts.FirstOrDefault(x => x.Id == fort.Id);
                    if (listedFort == null)
                    {
                        listedFort = fort;
                    }

                   
                    // Find the marker, add it to the overlay if it's new.
                    var thisMarker = markersOverlay.Markers.FirstOrDefault(x => x.Position == new PointLatLng(fort.Latitude, fort.Longitude));
                    if (thisMarker != null)
                    {
                        markersOverlay.Markers.Remove(thisMarker);
                        //thisMarker = new GMarkerGoogle(new PointLatLng(fort.Latitude, fort.Longitude), GMarkerGoogleType.blue);
                    }
                    

                    // Update the color of the marker.
                    GMarkerGoogleType markerType = GMarkerGoogleType.green;
                    if (fort.CooldownCompleteTimestampMs > unixCurrentTime)
                    {
                        markerType = GMarkerGoogleType.white_small;
                    }
                    else
                    {
                        markerType = GMarkerGoogleType.blue_small;
                    }

                    GMarkerGoogle updatedMarker = new GMarkerGoogle(new PointLatLng(fort.Latitude, fort.Longitude),
                        markerType
                      );

                    // Assign the updated marker to the overlay collection.
                    markersOverlay.Markers.Add(updatedMarker);
                    if (maxLatitude == null)
                    {
                        maxLatitude = fort.Latitude;
                        minLatitude = fort.Latitude;
                        maxLongitude = fort.Longitude;
                        minLongitude = fort.Longitude;
                    }

                    //thisMarker = updatedMarker;
                    maxLatitude = Math.Max((double)fort.Latitude, (double)maxLatitude);
                    minLatitude = Math.Min(fort.Latitude, (double)minLatitude);
                    maxLongitude = Math.Max(fort.Longitude, (double)maxLongitude);
                    minLongitude = Math.Min(fort.Longitude, (double)minLongitude);

                }


                if (maxLatitude != null)
                {
                    // Centre the map
                    double centreLatitude = (double)(minLatitude + ((maxLatitude - minLatitude) / 2));
                    double centreLongitude = (double)(minLongitude + ((maxLongitude - minLongitude) / 2));

                    gmap.Position = new PointLatLng(centreLatitude, centreLongitude);
                }
            }
        }


        private void gmap_Load(object sender, EventArgs e)
        {

        }

        private void Map_Load(object sender, EventArgs e)
        {

        }
    }
}
