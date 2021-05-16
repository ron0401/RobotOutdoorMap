using System;
using System.IO;
using CommandLine;
using CommandLine.Text;
using RobotOutdoorMapNET;
using System.Text.Json;

namespace map
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<NewCommand, MakeRouteCommand, AddRouteCommand>(args)
                .WithParsed<NewCommand>(opt => { opt.Execute(); })
                .WithParsed<MakeRouteCommand>(opt => { opt.Execute(); })
                .WithParsed<AddRouteCommand>(opt => { opt.Execute(); })
                .WithNotParsed(er => { /**/ })
                ;
        }
    }

    [Verb("new")]
    class NewCommand : MapBaseOptions
    {
        [Option('d', "datum", Required = true)]
        public string DatumString { get; set; }

        public override void Execute()
        {
            var map = new GlobalMap();
            map.Datum.Latitude = new CoordinateNET.Latitude() { Value = double.Parse(DatumString.Split(",")[0])};
            map.Datum.Longitude = new CoordinateNET.Longitude() { Value = double.Parse(DatumString.Split(",")[1])};
            map.SaveJson(this.MapFilePath);
        }
    }

    [Verb("makeroute")]
    class MakeRouteCommand : IExecutable
    {
        [Option('o', "output", Required = true)]
        public string OutputFilePath { get; set; }
        
        [Option('i', "input", Required = true)]
        public string NmeaFilePath { get; set; }

        public void Execute()
        {
            var r = new GlobalRoute(this.NmeaFilePath);
            File.WriteAllText(this.OutputFilePath, r.GetJson());
        }
    }


    [Verb("addroute")]
    class AddRouteCommand : MapBaseOptions
    {
        [Option('i', "input",  Required = true)]
        public string RouteFilePath { get; set; }

        public override void Execute()
        {
            var route = GlobalRoute.LoadJson(File.ReadAllText(this.RouteFilePath));
            var map = GlobalMap.LoadJson(File.ReadAllText(this.MapFilePath));
            map.Add(route);
            map.SaveJson(this.MapFilePath);
        }
    }

    abstract class MapBaseOptions : IExecutable
    {
        [Option('m', "mapfile", Required = true)]
        public string MapFilePath { get; set; }
        public FileInfo MapFile { get { return new FileInfo(this.MapFilePath); } }

        public abstract void Execute();
    }


    interface IExecutable 
    {
        void Execute();
    }
}
