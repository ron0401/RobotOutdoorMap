using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoordinateNET;

namespace RobotOutdoorMapNET
{

    public class GlobalMapSchema : SuperGlobalRouteSchema<GlobalMap>, ISchema
    {
        public GlobalMapSchema(GlobalMap map) :base(map.Points)
        {                 
            this.datum.Latitude = map.Datum.Latitude;
            this.datum.Longitude = map.Datum.Longitude;
            this.datum.Rotation = map.DatumRotation;
            this.ID = map.ID;
        }
        public GlobalMapSchema() 
        {

        }

        public GlobalMap GetGlobalMap()
        {
            return GetParent();
        }

        public override GlobalMap GetParent()
        {
            var map = new GlobalMap();
            map.Points = GetGlobalWayPoints();
            for (int i = 0; i < map.Points.Count; i++)
            {
                map.Points[i].SetLinkMap(map);
            }
            map.Datum = new GEO2d() { Latitude = this.datum.Latitude, Longitude = this.datum.Longitude };
            map.DatumRotation = this.datum.Rotation;
            map.ID = this.ID;
            return map;
        }

        //public List<GlobalWayPoint> GetLinkedWaypointList(string id,GlobalMap map) 
        //{
        //    var list = new List<string>();
        //    foreach (var f in this.Links)
        //    {
        //        if (id == f.LinkedPointID_A)
        //        {
        //            list.Add(f.LinkedPointID_B);
        //        }
        //    }
        //    var pointList = new List<GlobalWayPoint>();
        //    foreach (var f in map.Points)
        //    {
        //        foreach (var r in list)
        //        {
        //            if (f.ID == r)
        //            {
        //                pointList.Add(f);
        //            }
        //        }
        //    }

        //    return pointList;
        //}
        public class Datum
        {
            public Latitude Latitude { get; set; } = new Latitude();
            public Longitude Longitude { get; set; } = new Longitude();
            public double Rotation { get; set; }
        }
        public Datum datum { get; set; } = new Datum();
    }
}
