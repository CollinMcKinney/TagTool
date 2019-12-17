﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTool.Common;
using TagTool.IO;
using TagTool.Serialization;
using TagTool.Tags;

namespace TagTool.Cache
{
    public abstract class GameCache
    {
        public CacheVersion Version;

        public abstract TagCacheTest TagCache { get; }

        public static GameCache Open(FileInfo file)
        {
            MapFile map = new MapFile();
            var estimatedVersion = CacheVersion.HaloOnline106708;

            using (var stream = file.OpenRead())
            using (var reader = new EndianReader(stream))
            {
                if (file.Name.Contains(".map"))
                {
                    map.Read(reader);
                    estimatedVersion = map.Version;
                }
                else if (file.Name.Equals("tags.dat"))
                    estimatedVersion = CacheVersion.HaloOnline106708;
                else
                    throw new Exception("Invalid file passed to GameCache constructor");
            }

            switch (estimatedVersion)
            {
                case CacheVersion.HaloPC:
                case CacheVersion.HaloXbox:
                case CacheVersion.Halo2Vista:
                case CacheVersion.Halo2Xbox:
                    throw new Exception("Not implemented!");

                case CacheVersion.Halo3Beta:
                case CacheVersion.Halo3ODST:
                case CacheVersion.Halo3Retail:
                case CacheVersion.HaloReach:
                    return new GameCacheContextGen3(map, file);

                case CacheVersion.HaloOnline106708:
                case CacheVersion.HaloOnline235640:
                case CacheVersion.HaloOnline301003:
                case CacheVersion.HaloOnline327043:
                case CacheVersion.HaloOnline372731:
                case CacheVersion.HaloOnline416097:
                case CacheVersion.HaloOnline430475:
                case CacheVersion.HaloOnline454665:
                case CacheVersion.HaloOnline449175:
                case CacheVersion.HaloOnline498295:
                case CacheVersion.HaloOnline530605:
                case CacheVersion.HaloOnline532911:
                case CacheVersion.HaloOnline554482:
                case CacheVersion.HaloOnline571627:
                case CacheVersion.HaloOnline700123:
                    var directory = file.Directory.FullName;
                    var tagsPath = Path.Combine(directory, "tags.dat");
                    var tagsFile = new FileInfo(tagsPath);

                    if (!tagsFile.Exists)
                        throw new Exception("Failed to find tags.dat");

                    //CacheContext = new GameCacheContextHaloOnline(tagsFile.Directory);
                    break;
            }

            return null;
        }

        public abstract Stream OpenCacheRead();
        public abstract Stream OpenTagCacheRead();


        public abstract object Deserialize(Stream stream, CachedTag instance);
        public abstract T Deserialize<T>(Stream stream, CachedTag instance);

    }
    /*
    public class TagTable<T> : List<T> where T : ICachedTag
    {

    }
    */
    public interface ITagCache
    {
        List<ICachedTag> GetGenericTagTable();
        ICachedTag GetTagByName(string name, Tag groupTag);
        ICachedTag GetTagByIndex(int index);
        ICachedTag GetTagByID(int ID);
    }

    public interface IResourceCache
    {

    }

    public interface ITagSerialization
    {
        object Deserialize(Stream stream, ICachedTag instance);
        T Deserialize<T>(Stream stream, ICachedTag instance);
    }

    public interface ICacheFile
    {
        Stream OpenCacheRead();
        Stream OpenTagCacheRead();
    }

    public interface ICachedTag
    {
        string GetName();
        TagGroup GetTagGroup();
        int GetIndex();
        int GetID();
        int GetOffset();
    }

    //
    // New design
    //

    public abstract class CachedTag
    {
        public string Name;
        public int Index;
        public uint ID;
        public TagGroup Group;

        public abstract uint DefinitionOffset { get; }

        public override string ToString()
        {
            if(Name == null)
                return $"0x{Index.ToString("X8")}.{Group.ToString()}";
            else
                return $"{Name}.{Group.ToString()}";
        }

        public bool IsInGroup(params Tag[] groupTags)
        {
            return Group.BelongsTo(groupTags);
        }
    }

    public abstract class TagCacheTest
    {
        public virtual IEnumerable<CachedTag> TagTable { get;}

        public abstract CachedTag GetTagByID(int ID);
        public abstract CachedTag GetTagByIndex(int index);
        public abstract CachedTag GetTagByName(string name, Tag groupTag);

    }
}