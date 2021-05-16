using System;
using System.IO;
using System.Collections.Generic;
using CoordinateNET;
using System.Text.Json;
using System.Diagnostics;

namespace RobotOutdoorMapNET
{
    public abstract class SchemaConverter<T ,Schema> : HaveID
    {
        public string GetJson() 
        {
            string json = JsonSerializer.Serialize(this.GetSchema(), new JsonSerializerOptions() { WriteIndented = true });
            return json;
        }
        public void SaveJson(string filepath) 
        {
            File.WriteAllText(filepath,this.GetJson());
        }

        public abstract Schema GetSchema();

    }

    public abstract class Schema<Inheritance> : HaveID
    {
        
    }
    public abstract class Map<T, Schema> : SuperRoute<T, Schema>
    {

    }
    public abstract class SuperRoute<T, Schema> : SchemaConverter<T, Schema> 
    {
        public List<GlobalWayPoint> Points { get; set; } = new List<GlobalWayPoint>();

    }
    public abstract class WayPoint<T,Schema> : SchemaConverter<T,Schema> 
    {
    
    }
    public abstract class HaveID
    {
        internal string _id = Guid.NewGuid().ToString();
        public string ID { get { return _id; } set { _id = value; } } 
    }
    public class GlobalWayPointSchema : HaveID
    {
        public GlobalWayPointSchema(GlobalWayPoint point) 
        {
            this._id = point.ID;
            this.GEO = point.GEO;
        }
        public GlobalWayPointSchema()
        {

        }
        public GEO2d GEO { get; set; }
        public static List<GlobalWayPointSchema> GetWayPointSchemas(List<GlobalWayPoint> points) 
        {
            var list = new List<GlobalWayPointSchema>();
            foreach (var f in points)
            {
                list.Add(new GlobalWayPointSchema(f));
            }
            return list;
        }
    }

    public class LinkSchema : Schema<Link>
    {
        public LinkSchema()
        {

        }
        public static List<LinkSchema> GetLinkSchemas(List<GlobalWayPoint> wayPoints) 
        {
            var list = new List<LinkSchema>();
            foreach (var f in wayPoints)
            {
                foreach (var t in f.Links)
                {
                    list.Add(new LinkSchema()
                    {
                        LinkedPointID_A = f.ID,
                        LinkedPointID_B = t.LinkedPoint.ID
                    });
                }
            }
            return list;
        }
        public string LinkedPointID_A { get; set; }
        public string LinkedPointID_B { get; set; }
    }

    public class GlobalMap : Map<GlobalMap,GlobalMapSchema>, IGlobalRouteHaving
    {
        public void Add(GlobalRoute route)
        {
            this.Add(route, false, 0);
        }
        public void Add(GlobalRoute route, double jointThreshold)
        {
            this.Add(route, true, jointThreshold);
        }
        private void Add(GlobalRoute route , bool jointNearWaypoint , double jointThreshold) 
        {
            if (this.Datum == null)
            {
                this.Datum = route.Points[0].GEO;
            }
            if (this.Points.Count == 0)
            {
                foreach (var f in route.Points)
                {
                    f.SetLinkMap(this);
                    this.Points.Add(f);
                }
                return;
            }
            if (jointNearWaypoint)
            {
                var p = FindNearWaypoint(route.Points[0], jointThreshold);
                if (p != null)
                {
                    p.Links.Add(new Link(route.Points[0]));
                }
                p = FindNearWaypoint(route.Points[route.Points.Count - 1], jointThreshold);
                if (p != null)
                {
                    route.Points[route.Points.Count - 1].Links.Add(new Link(p));
                }
            }
            foreach (var f in route.Points)
            {
                f.SetLinkMap(this);
                this.Points.Add(f);
            }
        }
        internal GlobalWayPoint FindNearWaypoint(GlobalWayPoint point, double threshold) 
        {
            var p = this.Points[0];
            foreach (var f in this.Points)
            {
                if (f.GEO.ConvertToENU(point.GEO).Length < p.GEO.ConvertToENU(point.GEO).Length)
                {
                    p = f;
                }
            }
            if (p.GEO.ConvertToENU(point.GEO).Length > threshold)
            {
                return null;
            }
            return p;
        }
        public static GlobalMap LoadJson(string json)
        {
            var map = JsonSerializer.Deserialize<GlobalMapSchema>(json);
            return map.GetGlobalMap();   
        }
        public override GlobalMapSchema GetSchema()
        {
            return new GlobalMapSchema(this);
        }

        public GlobalMap() 
        {
            
        }
       
        public GEO2d Datum { get; set; } = new GEO2d();
        public double DatumRotation { get; set; } 

    }
    public class Link
    {
        public Link(GlobalWayPoint point) 
        {
            _linkedPoint = point;
        }
        private GlobalWayPoint _linkedPoint;
        public string LinkedWaypointID { get; set; }
        public GlobalWayPoint LinkedPoint { get { return _linkedPoint; } }
    }
    public abstract class BaseWayPoint 
    {
        internal string _id;
        public string ID { get { return _id; } set { _id = value; } }
    }
    public class GlobalWayPoint:BaseWayPoint
    {
        public GlobalWayPoint(GEO2d geo, GlobalMap map) 
        {
            _map = map;
            _geo = geo;
        }
        public GlobalWayPoint(GEO2d geo)
        {
            _geo = geo;
            _id = Guid.NewGuid().ToString();
        }
        private GEO2d _geo;
        private GlobalMap _map;
        public List<Link> Links { get; set; } = new List<Link>();
        public GEO2d GEO { get { return _geo; } }
        public GlobalMap LinkMap { get { return _map; }  }
        internal void SetLinkMap(GlobalMap map) 
        {
            _map = map;
        }
    }

    public class TargetPoint
    {

    }

    public class GeoPoints
    {
        public List<GEO2d> Points { get; set; } = new List<GEO2d>();
        public void ThinOut(double th) 
        {
            var list = new List<GEO2d>();
            list.Add(Points[0]);
            var target = Points[0];
            foreach (var f in Points)
            {
                if (f.GetDistance(target) >= th)
                {
                    list.Add(f);
                    target = f;
                }
            }
            this.Points = list;
        }
    }

}
