using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoordinateNET;
using System.IO;
using NMEAParserNET;
using System.Text.Json;


namespace RobotOutdoorMapNET
{
    public class GlobalRouteSchema: SuperGlobalRouteSchema<GlobalRoute>
    {
        public GlobalRouteSchema(List<GlobalWayPoint> wayPoints):base(wayPoints)
        {
            
        }

        public GlobalRouteSchema() 
        {

        }
        public override GlobalRoute GetParent()
        {
            var route = new GlobalRoute();
            route.Points = GetGlobalWayPoints();
            return route;
        }
    }

    public abstract class SuperGlobalRouteSchema<Parent> : HaveID 
    {
        public SuperGlobalRouteSchema() 
        {
        }
        public SuperGlobalRouteSchema(List<GlobalWayPoint> waypoints)
        {
            WayPoints = GlobalWayPointSchema.GetWayPointSchemas(waypoints);
            Links = LinkSchema.GetLinkSchemas(waypoints);
        }
        public abstract Parent GetParent(); 
        public List<GlobalWayPointSchema> WayPoints { get; set; } = new List<GlobalWayPointSchema>();
        public List<LinkSchema> Links { get; set; } = new List<LinkSchema>();

        internal List<GlobalWayPoint> GetGlobalWayPoints ()
        {
            var v = new List<GlobalWayPoint>();
            foreach (var f in this.WayPoints)
            {
                v.Add(new GlobalWayPoint(f.GEO) { ID = f.ID });
            }
            foreach (var f in v)
            {
                var list = GetLinkedWaypointList(f.ID, v);
                foreach (var r in list)
                {
                    f.Links.Add(new Link(r));
                }
            }
            return v;
        }
        public List<GlobalWayPoint> GetLinkedWaypointList(string id, List<GlobalWayPoint> points)
        {
            var list = new List<string>();
            foreach (var f in this.Links)
            {
                if (id == f.LinkedPointID_A)
                {
                    list.Add(f.LinkedPointID_B);
                }
            }
            var pointList = new List<GlobalWayPoint>();
            foreach (var f in points)
            {
                foreach (var r in list)
                {
                    if (f.ID == r)
                    {
                        pointList.Add(f);
                    }
                }
            }

            return pointList;
        }
    }

    public interface IGlobalRouteHaving
    {
        List<GlobalWayPoint> Points { get; set; } 
    }
    public class GlobalRoute : SuperRoute<GlobalRoute,GlobalRouteSchema>
    {
        public GlobalRoute() 
        {

        }
        public static GlobalRoute LoadJson(string json)
        {
            var map = JsonSerializer.Deserialize<GlobalRouteSchema>(json);
            return map.GetParent();
        }
        class StraightLine 
        {
            public StraightLine(GEO2d geo1,GEO2d geo2) 
            {
                datum = geo1;
                var enu1 = geo1.ConvertToENU(geo1);
                var enu2 = geo2.ConvertToENU(geo1);
                a = (enu2.Y - enu1.Y) / (enu2.X - enu1.X);
                b = enu2.Y - this.a * enu2.X;
            }
            public double a { get; set; }
            public double b { get; set; }
            private GEO2d datum;
            public double GetDistance(GEO2d geo) 
            {
                var enu = geo.ConvertToENU(datum);
                return Math.Abs(-1 * a * enu.X + enu.Y + -1 * b) / Math.Sqrt(Math.Pow(a, 2) + 1);
            }
        }
        public GlobalRoute(string nmeaPath):this(getGEOList(nmeaPath))
        {
             
        }
        private static List<GEO2d> getGEOList(string nmeaPath) 
        {
            var list = new List<GEO2d>();
            string[] ary = File.ReadAllLines(nmeaPath);
            foreach (var f in ary)
            {
                if (NMEAParserNET.Parser.JudgeMsgType(f) == NMEAParserNET.NMEA.Sentence.TypeOfMessage.GGA)
                {
                    var gga = new NMEAParserNET.NMEA.GGA(f);
                    if (gga.Latitude != null && gga.Longitude != null)
                    {
                        list.Add(gga.GEO2d);
                    }

                }
            }
            return list;
        }

        public override GlobalRouteSchema GetSchema()
        {
            return new GlobalRouteSchema(this.Points);
        }

        public GlobalRoute(List<GEO2d> GEOs) 
        {
            Points.Add(new GlobalWayPoint(GEOs[0]));
            Points.Add(new GlobalWayPoint(GEOs[1]));
            foreach (var f in GEOs)
            {
                var line = new StraightLine(Points[Points.Count -2].GEO, Points[Points.Count - 1].GEO);
                if (f.ConvertToENU(Points[Points.Count - 1].GEO).Length >= 10 || ( line.GetDistance(f) >= 0.6 && f.ConvertToENU(Points[Points.Count - 1].GEO).Length >= 1))
                {
                    Points.Add(new GlobalWayPoint(f));
                    Points[Points.Count - 2].Links.Add(new Link(Points[Points.Count - 1]));
                }
            }
            Points.RemoveAt(0);
        }
    }
}
