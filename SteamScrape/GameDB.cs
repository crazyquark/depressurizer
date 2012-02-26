﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using DPLib;

namespace SteamScrape {
    class GameDB {
        public Dictionary<int, GameDBEntry> Games = new Dictionary<int, GameDBEntry>();

        public void FetchAppList() {
            XmlDocument doc = new XmlDocument();
            WebRequest req = WebRequest.Create( @"http://api.steampowered.com/ISteamApps/GetAppList/v0002/?format=xml" );
            using( WebResponse resp = req.GetResponse() ) {

                doc.Load( resp.GetResponseStream() );
            }
            foreach( XmlNode node in doc.SelectNodes( "/applist/apps/app" ) ) {
                int appId;
                if( !XmlUtil.GetIntFromNode( node["appid"], out appId ) ) {
                    continue;
                }
                string name;
                XmlUtil.GetStringFromNode( node["name"], out name );

                GameDBEntry g = new GameDBEntry();
                g.Id = appId;
                g.Name = name;

                if( !Games.ContainsKey( appId ) ) {
                    Games.Add( appId, g );
                }
            }
        }

        public void SaveToXml( string path ) {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.CloseOutput = true;
            XmlWriter writer = XmlWriter.Create( path, settings );
            writer.WriteStartDocument();
            writer.WriteStartElement( "gamelist" );
            foreach( GameDBEntry g in Games.Values ) {
                writer.WriteStartElement( "game" );

                writer.WriteElementString( "id", g.Id.ToString() );
                if( !string.IsNullOrEmpty( g.Name ) ) {
                    writer.WriteElementString( "name", g.Name );
                }
                writer.WriteElementString( "type", g.Type.ToString() );
                if( !string.IsNullOrEmpty( g.Genre ) ) {
                    writer.WriteElementString( "genre", g.Genre );
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        public void LoadFromXml( string path ) {
            XmlDocument doc = new XmlDocument();
            doc.Load( path );

            Games.Clear();

            foreach( XmlNode gameNode in doc.SelectNodes( "/gamelist/game" ) ) {
                int id;
                if( !XmlUtil.GetIntFromNode( gameNode["id"], out id ) || Games.ContainsKey( id ) ) {
                    continue;
                }
                GameDBEntry g = new GameDBEntry();
                g.Id = id;
                XmlUtil.GetStringFromNode( gameNode["name"], out g.Name );
                XmlNode typeNode = gameNode["type"];
                string typeString;
                if( !XmlUtil.GetStringFromNode( gameNode["type"], out typeString ) || !Enum.TryParse<AppType>( typeString, out g.Type ) ) {
                } else {
                    g.Type = AppType.Unknown;
                }
                Games.Add( id, g );
            }
        }
    }
}