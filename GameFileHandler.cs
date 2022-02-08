using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ProtoBuf;
using s4pi.Package;
using s4pi.Interfaces;
using s4pi.ImageResource;
using System.Collections.Generic;

namespace TS4SimRipper
{
    public partial class Form1 : Form
    {
        Package[] gamePackages = new Package[0];
        string[] gamePackageNames = new string[0];
        bool[] notBaseGame = new bool[0];
        public Package TroubleshootPackageTuning = (Package)Package.NewPackage(0);
        Package TroubleshootPackageBasic = (Package)Package.NewPackage(0);
        Package TroubleshootPackageOutfit = (Package)Package.NewPackage(0);
        public Dictionary<(uint, uint, ulong), (string, Package)> allMaxisInstances = new Dictionary<(uint, uint, ulong), (string, Package)>();
        public Dictionary<(uint, uint, ulong), (string, Package)> allCCInstances = new Dictionary<(uint, uint, ulong), (string, Package)>();
        public Dictionary<(uint, uint, ulong), (string, Package)> allInstances = new Dictionary<(uint, uint, ulong), (string, Package)>();


        private bool DetectFilePaths()
        {
            try
            {
                if (String.Compare(Properties.Settings.Default.TS4Path, " ") <= 0)
                {
                    string tmp = (string)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Maxis\\The Sims 4", "Install Dir", null);
                    if (tmp != null) Properties.Settings.Default.TS4Path = tmp;
                    //MessageBox.Show(tmp);
                    Properties.Settings.Default.Save();
                }

                if (String.Compare(Properties.Settings.Default.TS4ModsPath, " ") <= 0)
                {
                    string tmp = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Electronic Arts\\The Sims 4\\Mods";
                    if (!Directory.Exists(tmp))
                    {
                        string[] tmp2 = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Electronic Arts\\", "*Sims 4*", SearchOption.AllDirectories);
                        if (tmp2.Length == 1)
                        {
                            // tmp = Path.GetDirectoryName(tmp2[0]) + Path.DirectorySeparatorChar;
                            tmp = tmp2[0] + Path.DirectorySeparatorChar + "Mods" + Path.DirectorySeparatorChar;
                        }
                        else if (tmp2.Length > 1)
                        {
                            if (tmp2[0].Length < tmp2[1].Length)
                                tmp = tmp2[0] + Path.DirectorySeparatorChar + "Mods" + Path.DirectorySeparatorChar;
                            //tmp = Path.GetDirectoryName(tmp2[0]) + Path.DirectorySeparatorChar;
                            else
                                tmp = tmp2[1] + Path.DirectorySeparatorChar + "Mods" + Path.DirectorySeparatorChar;
                            //tmp = Path.GetDirectoryName(tmp2[1]) + Path.DirectorySeparatorChar;
                        }
                    }
                    if (tmp != null) Properties.Settings.Default.TS4ModsPath = tmp;
                }

                if (String.Compare(Properties.Settings.Default.TS4SavesPath, " ") <= 0)
                {
                    string tmp = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Electronic Arts\\The Sims 4\\saves";
                    if (!Directory.Exists(tmp))
                    {
                        string[] tmp2 = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Electronic Arts\\", "*Sims 4*", SearchOption.AllDirectories);
                        if (tmp2.Length == 1)
                        {
                            // tmp = Path.GetDirectoryName(tmp2[0]) + Path.DirectorySeparatorChar;
                            tmp = tmp2[0] + Path.DirectorySeparatorChar + "saves";
                        }
                        else if (tmp2.Length > 1)
                        {
                            if (tmp2[0].Length < tmp2[1].Length)
                                tmp = tmp2[0] + Path.DirectorySeparatorChar + "saves";
                            //tmp = Path.GetDirectoryName(tmp2[0]) + Path.DirectorySeparatorChar;
                            else
                                tmp = tmp2[1] + Path.DirectorySeparatorChar + "saves";
                            //tmp = Path.GetDirectoryName(tmp2[1]) + Path.DirectorySeparatorChar;
                        }
                    }
                    if (tmp != null) Properties.Settings.Default.TS4SavesPath = tmp; Properties.Settings.Default.Save();
                }

                if (Properties.Settings.Default.TS4Path == null || Properties.Settings.Default.TS4ModsPath == null) return false;
            }
            catch (Exception e)
            {
                return false;
            }
            return (Directory.Exists(Properties.Settings.Default.TS4Path) & Directory.Exists(Properties.Settings.Default.TS4ModsPath));
        }

        private bool SetupGamePacks()
        {
            string TS4FilesPath = Properties.Settings.Default.TS4Path;
            List<Package> gamePacks = new List<Package>();
            List<string> paths = new List<string>();
            List<bool> notBase = new List<bool>();
            try
            {
                List<string> pathsSim = new List<string>(Directory.GetFiles(TS4FilesPath, "Simulation*Build*.package", SearchOption.AllDirectories));
                List<string> pathsClient = new List<string>(Directory.GetFiles(TS4FilesPath, "Client*Build*.package", SearchOption.AllDirectories));
                List<string> pathsNoLegacySim = new List<string>();
                List<string> pathsNoLegacyClient = new List<string>();

                for (int i = 0; i < pathsSim.Count; i++)
                {
                    if (!pathsSim[i].Contains("Delta_LE"))
                    {
                        pathsNoLegacySim.Add(pathsSim[i]);
                    }
                }
                for (int i = 0; i < pathsClient.Count; i++)
                {
                    if (!pathsClient[i].Contains("Delta_LE"))
                    {
                        pathsNoLegacyClient.Add(pathsClient[i]);
                    }
                }


                pathsNoLegacySim.Sort();
                pathsNoLegacyClient.Sort();
                paths.AddRange(pathsNoLegacySim);
                paths.AddRange(pathsNoLegacyClient);
            }
            catch (DirectoryNotFoundException e)
            {
                MessageBox.Show("Your game packages path is invalid! Please go to File / Change Settings and correct it or make it blank to reset, then restart." 
                    + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                return false;
            }
            catch (IOException e)
            {
                MessageBox.Show("Your game packages path is invalid or a network error has occurred! Please go to File / Change Settings and correct it or make it blank to reset, then restart."
                    + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                return false;
            }
            catch (ArgumentException e)
            {
                MessageBox.Show("Your game packages path is not specified correctly! Please go to File / Change Settings and correct it or make it blank to reset, then restart."
                    + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                return false;
            }
            catch (UnauthorizedAccessException e)
            {
                MessageBox.Show("You do not have the required permissions to access the game packages folder! Please restart with admin privileges."
                    + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                return false;
            }
            if (paths.Count == 0)
            {
                MessageBox.Show("Can't find game packages! Please go to File / Change Settings and correct the game packages path or make it blank to reset, then restart.");
                return false;
            }

            Predicate<IResourceIndexEntry> testPred = r => r.ResourceType == (uint)ResourceTypes.Rig;
            try
            {
                string err = "";
                for (int i = 0; i < paths.Count; i++)
                {
                    if (paths[i].Contains("Delta_LE"))
                    {
                        continue;
                    }

                    try
                    {
                        Package p = OpenPackage(paths[i], false);
                        if (p == null)
                        {

                            err += paths[i] + " is NULL" + Environment.NewLine;
                            paths[i] = null;
                            continue;
                        }
                        else
                        {
                            foreach (IResourceIndexEntry indexEntry in p.GetResourceList)
                            {

                                (uint ResourceType, uint ResourceGroup, ulong Instance) key = (indexEntry.ResourceType, indexEntry.ResourceGroup, indexEntry.Instance);

                                if (!this.allMaxisInstances.ContainsKey(key))
                                {
                                    this.allMaxisInstances.Add(key, (paths[i], p));
                                }

                                if (!this.allInstances.ContainsKey(key))
                                {
                                    this.allInstances.Add(key, (paths[i], p));
                                }
                            }
                        }
                        gamePacks.Add(p);
                        notBase.Add(!paths[i].Contains("\\Data\\"));
                    }

                    catch (Exception e)
                    {
                        err += paths[i] + " : " + e.Message + Environment.NewLine;
                        paths[i] = null;
                    }
                }


                if (err.Length > 0) MessageBox.Show("Unable to open the following game packages:" + Environment.NewLine + err);

                List<Package> ccPacks = new List<Package>();
                List<string> ccPaths = new List<string>();
                List<bool> isCC = new List<bool>();
                string[] localCC = null;
                try
                {
                    localCC = Directory.GetFiles(Properties.Settings.Default.TS4ModsPath,
                        "*.package", SearchOption.AllDirectories);
                }
                catch
                {
                    MessageBox.Show("Either the path to your user Sims 4 folder in Documents is incorrect or you have no custom content." +
                        Environment.NewLine + "If the path is incorrect, go to File / Change Settings and correct it or make it blank to reset, then restart.");
                }
                if (localCC != null)
                {
                    ccPaths = new List<string>(localCC);
                    // ccPaths.Sort((a, b) => b.CompareTo(a));     //descending sort
                    ccPaths.Sort();
                    err = "";
                    for (int j = 0; j < ccPaths.Count; j++)
                    {
                        try
                        {
                            Package p = OpenPackage(ccPaths[j], false);
                            if (p == null)
                            {
                                err += ccPaths[j] + " is NULL" + Environment.NewLine;
                                ccPaths[j] = null;
                                continue;
                            }
                            else
                            {
                                foreach (IResourceIndexEntry indexEntry in p.GetResourceList)
                                {
                                    (uint ResourceType, uint ResourceGroup, ulong Instance) key = (indexEntry.ResourceType, indexEntry.ResourceGroup, indexEntry.Instance);

                                    if (!this.allCCInstances.ContainsKey(key))
                                    {
                                        this.allCCInstances.Add(key, (paths[j], p));

                                    }
                                    if (!this.allInstances.ContainsKey(key))
                                    {
                                        this.allInstances.Add(key, (paths[j], p));
                                    }
                                }
                            }
                            ccPacks.Add(p);
                            isCC.Add(true);
                        }
                        catch (Exception e)
                        {
                            err += ccPaths[j] + " : " + e.Message + Environment.NewLine;
                            ccPaths[j] = null;
                        }
                    }
                    if (err.Length > 0) MessageBox.Show("Unable to open the following mod packages:" + Environment.NewLine + err);
                }

                ccPacks.AddRange(gamePacks);
                ccPaths.AddRange(paths);
                isCC.AddRange(notBase);
                ccPaths.RemoveAll(item => item == null);
                gamePackages = ccPacks.ToArray();
                gamePackageNames = ccPaths.ToArray();
                notBaseGame = isCC.ToArray();
            }
            catch (UnauthorizedAccessException e)
            {
                MessageBox.Show("You do not have the required permissions to open the game and/or mod packages! Please restart with admin privileges."
                    + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace);
                return false;
            }
            return true;
        }

        private void SaveStream(IResourceIndexEntry irie, BinaryReader br, Package pack)
        {
            Stream s = new MemoryStream();
            br.BaseStream.CopyTo(s);
            s.Position = 0;
            pack.AddResource(irie, s, true);
        }
        private void SaveStream(TGI tgi, Stream r, Package pack)
        {
            Stream s = new MemoryStream();
            r.CopyTo(s);
            s.Position = 0;
            TGIBlock tgiblock = new TGIBlock(1, null, tgi.Type, tgi.Group, tgi.Instance);
            pack.AddResource(tgiblock, s, true);
        }
        private void SaveStream(IResourceIndexEntry irie, Stream r, Package pack)
        {
            Stream s = new MemoryStream();
            r.CopyTo(s);
            s.Position = 0;
            pack.AddResource(irie, s, true);
        }

        private Sculpt FetchGameSculpt(TGI tgi, ref string errorMsg)
        {
            if (tgi.Instance == 0ul) return null;
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);

            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == tgi.Type & r.Instance == tgi.Instance;
            if (this.allInstances.ContainsKey(key))
            {  
                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                    if (irie != null)
                    {
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        if (this.allCCInstances.ContainsKey(key)) SaveStream(irie, br, TroubleshootPackageBasic);
                        try
                        {
                            Sculpt sculpt = new Sculpt(br);
                            return sculpt;
                        }
                        catch (Exception e)
                        {
                            errorMsg += "Can't read Sculpt " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1) + " : " + e.Message + Environment.NewLine;
                            return null;
                        }
                    }
                }
            }
            errorMsg += "Can't find Sculpt " + tgi.ToString() + Environment.NewLine;
            return null;
        }
        private SMOD FetchGameSMOD(TGI tgi, ref string errorMsg)
        {
            if (tgi.Instance == 0ul) return null;
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);
            if (this.allInstances.ContainsKey(key)) { 
                Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        if (this.allCCInstances.ContainsKey(key)) SaveStream(irie, br, TroubleshootPackageBasic);
                        try
                        {
                            SMOD smod = new SMOD(br);
                            return smod;
                        }
                        catch (Exception e)
                        {
                            errorMsg += "Can't read SimModifier " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1) + " : " + e.Message + Environment.NewLine;
                            return null;
                        }
                    }
                }
            }
            errorMsg += "Can't find SimModifier " + tgi.ToString() + Environment.NewLine;
            return null;
        }
        private BGEO FetchGameBGEO(TGI tgi, ref string errorMsg)
        {

            if (tgi.Instance == 0ul) return null;
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);

            if (this.allInstances.ContainsKey(key))
            {
                string err = "";
                Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;

                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);

                if (irie != null)
            {
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        if (this.allCCInstances.ContainsKey(key)) SaveStream(irie, br, TroubleshootPackageBasic);
                        try
                        { 
                        BGEO bgeo = new BGEO(br);
                        return bgeo;
                    }
                    catch (Exception e)
                    {

                         errorMsg += "Can't read BGEO " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1) + " : " + e.Message + Environment.NewLine;
                        return null;
                    }
                }
              }
            }
            errorMsg += "Can't find BGEO " + tgi.ToString() + Environment.NewLine;
            return null;
        }
        private DMap FetchGameDMap(TGI tgi, ref string errorMsg)
        {

            if (tgi.Instance == 0ul) return null;
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);
            {
                string err = "";
                Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;

                if (this.allInstances.ContainsKey(key))
                {
                    Package p = this.allInstances[key].Item2;
                    IResourceIndexEntry irie = p.Find(pred);
                    if (irie != null)
                    {
                        using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                        {
                            if (this.allCCInstances.ContainsKey(key)) SaveStream(irie, br, TroubleshootPackageBasic);
                            try
                            {
                                DMap dmap = new DMap(br);
                                return dmap;
                            }
                            catch (Exception e)
                            {
                                errorMsg += "Can't read DMap " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1) + " : " + e.Message + Environment.NewLine;
                                return null;
                            }
                        }
                    }
                }
            }
            errorMsg += "Can't find DeformerMap " + tgi.ToString() + Environment.NewLine;
            return null;
        }

        private BOND FetchGameBOND(TGI tgi, ref string errorMsg)
        {

            if (tgi.Instance == 0ul) return null;

            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);

            if (this.allInstances.ContainsKey(key))
            {
                string err = "";
                Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;

                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        if (this.allCCInstances.ContainsKey(key)) SaveStream(irie, br, TroubleshootPackageBasic);
                        try
                        {
                            BOND bond = new BOND(br);
                            return bond;
                        }
                        catch (Exception e)
                        {
                            errorMsg += "Can't read BOND " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1) + " : " + e.Message + Environment.NewLine;
                            return null;
                        }
                    }
                }
            }
            errorMsg += "Can't find BoneDelta " + tgi.ToString() + Environment.NewLine;
            return null;
        }

        private CASP FetchGameCASP(TGI tgi, out string packageName, BodyType partType, int outfitNumber, ref string errorMsg, bool saveme)
        {

            if (tgi.Instance == 0ul) { packageName = ""; return null; }
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);

            if (this.allInstances.ContainsKey(key))
            {
                string err = "";
                Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;

                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        if (this.allCCInstances.ContainsKey(key) && saveme) SaveStream(irie, br, TroubleshootPackageOutfit);
                        try
                        {
                            CASP casp = new CASP(br);
                            casp.tgi = new TGI(irie.ResourceType, irie.ResourceGroup, irie.Instance);
                            casp.notBaseGame = this.allCCInstances.ContainsKey(key);
                            packageName = Path.GetFileName(allInstances[key].Item1);
                            return casp;
                        }
                        catch (Exception e)
                        {
                            errorMsg += "Can't read " + partType.ToString() + " CASP " + tgi.ToString() + ", Package: " + Path.GetFileName(allInstances[key].Item1) + " : " + e.Message + Environment.NewLine;
                            packageName = Path.GetFileName(this.allInstances[key].Item1);
                            if (this.allCCInstances.ContainsKey(key) && !saveme) SaveStream(irie, br, TroubleshootPackageOutfit);
                            return null;
                        }
                    }
                }
            }
            errorMsg += "Can't find " + partType.ToString() + " CASP " + tgi.ToString() + Environment.NewLine;
            packageName = "";
            return null;
        }
        private GEOM FetchGameGEOM(TGI tgi, string packname, int outfitNumber, ref string errorMsg)
        {

            if (tgi.Instance == 0ul) return null;
            string err = "";

            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);

            if (this.allInstances.ContainsKey(key))
            {
                Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;

                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        if (this.allCCInstances.ContainsKey(key)) SaveStream(irie, br, TroubleshootPackageOutfit);
                        try
                        {
                            GEOM geom = new GEOM(br);
                            geom.StandardizeFormat();
                            return geom;
                        }
                        catch (Exception e)
                        {
                            errorMsg += "Can't read GEOM " + tgi.ToString() + ", Package: " + this.allCCInstances.ContainsKey(key) + " : " + e.Message + Environment.NewLine;
                            return null;
                        }
                    }
                }
            }
            errorMsg += "Can't find GEOM mesh " + tgi.ToString() + ", Linked from package: " + packname + Environment.NewLine;
            return null;
        }
        private RegionMap FetchGameRMap(TGI tgi, string packname, int outfitNumber, ref string errorMsg)
        {
            if (tgi.Instance == 0ul) return null;
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);

            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == tgi.Type & r.Instance == tgi.Instance;
            if (this.allInstances.ContainsKey(key))
            {
                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        if (this.allCCInstances.ContainsKey(key)) SaveStream(irie, br, TroubleshootPackageOutfit);
                        try
                        {
                            RegionMap rmap = new RegionMap(br);
                            return rmap;
                        }
                        catch (Exception e)
                        {
                            errorMsg += "Can't read RMap " + tgi.ToString() + ", Package: " + this.allCCInstances.ContainsKey(key) + " : " + e.Message + Environment.NewLine;
                            return null;
                        }
                    }
                }
            }
            errorMsg += "Can't find RegionMap " + tgi.ToString() + ", Linked from package: " + packname + Environment.NewLine;
            return null;
        }
        private RLEResource FetchGameRLE(TGI tgi, int outfitNumber, ref string errorMsg)
        {
            if (tgi.Instance == 0ul) return null;
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);

            Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            if (this.allInstances.ContainsKey(key))
            {
                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    Stream s = p.GetResource(irie);
                    if (this.allCCInstances.ContainsKey(key)) SaveStream(irie, s, TroubleshootPackageOutfit);
                    try
                    {
                        RLEResource rle = new RLEResource(1, s);
                        return rle;
                    }
                    catch (Exception e)
                    {
                        errorMsg += "Can't read RLE " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1) + " : " + e.Message + Environment.NewLine;
                    }
                }
            }
            errorMsg += "Can't find RLE texture " + tgi.ToString() + Environment.NewLine;
            return null;
        }
        private Bitmap FetchGameImageFromRLE(TGI tgi, int outfitNumber, ref string errorMsg)
        {
            return FetchGameImageFromRLE(tgi, false, outfitNumber, ref errorMsg, true);
        }
        private Bitmap FetchGameImageFromRLE(TGI tgi, bool isSpecular, int outfitNumber, ref string errorMsg, bool writeLog)
        {
            if (tgi.Instance == 0ul) return null;
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);
            (uint ResourceType, uint ResourceGroup, ulong Instance) keyLrle = ((uint)ResourceTypes.LRLE, tgi.Group, tgi.Instance);


            Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            Predicate<IResourceIndexEntry> predLrle = r => r.ResourceType == (uint)ResourceTypes.LRLE & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            if (this.allInstances.ContainsKey(key) || this.allInstances.ContainsKey(keyLrle))
            {
                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                IResourceIndexEntry irieLrle = p.Find(predLrle);
                if (irie != null || irieLrle != null)
                {
                    try
                    {
                        Stream s = new MemoryStream();
                        if (irie != null)
                        {
                            s = p.GetResource(irie);
                        }
                        if (s.Length < 4)
                        {
                            if (irieLrle != null)
                            {
                                s = p.GetResource(irieLrle);
                            }
                        }
                        if (this.allCCInstances.ContainsKey(key)) SaveStream(irie, s, outfitNumber >= 0 ? TroubleshootPackageOutfit : TroubleshootPackageBasic);
                        using (RLEResource rle = new RLEResource(1, s))
                        {
                            if (rle != null && rle.AsBytes.Length > 0)
                            {
                                using (DdsFile dds = new DdsFile())
                                {
                                    dds.Load(rle.ToDDS(), false);
                                    Bitmap texture;
                                    if (isSpecular)
                                    {
                                        Size specSize = new Size(currentSize.Width / 2, currentSize.Height / 2);
                                        if (dds.Size == specSize) texture = new Bitmap(dds.Image);
                                        else texture = new Bitmap(dds.Image, specSize);
                                    }
                                    else
                                    {
                                        Bitmap tmp = dds.Image;
                                        if (tmp.Size == currentSize) texture = new Bitmap(tmp);
                                        else texture = new Bitmap(tmp, currentSize);
                                    }
                                    return texture;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (isSpecular && writeLog)
                        {
                            errorMsg += "Can't read RLES image " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1) + " : " + e.Message + Environment.NewLine;
                        }
                    }
                }
            }
            if (isSpecular)
            {
                if (writeLog) errorMsg += "Can't find RLES texture " + tgi.ToString() + Environment.NewLine;
            }
            else
            {
                if (writeLog) errorMsg += "Can't find RLE texture " + tgi.ToString() + Environment.NewLine;
            }
            return null;
        }
        private LRLE FetchGameLRLE(TGI tgi, int outfitNumber, ref string errorMsg)
        {
            string err = "";

            if (tgi.Instance == 0ul) return null;
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);

            Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            if (this.allInstances.ContainsKey(key))
            {
                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        if (this.allCCInstances.ContainsKey(key)) SaveStream(irie, br, TroubleshootPackageOutfit);
                        try
                        {
                            LRLE lrle = new LRLE(br);
                            return lrle;
                        }
                        catch (Exception e)
                        {
                            errorMsg += "Can't read LRLE " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1) + " : " + e.Message + Environment.NewLine;
                        }
                    }
                }
            }
            errorMsg += "Can't find LRLE texture " + tgi.ToString() + Environment.NewLine;
            return null;
        }
        private Bitmap FetchGameImageFromLRLE(TGI tgi, int outfitNumber, ref string errorMsg)
        {
            return FetchGameImageFromLRLE(tgi, outfitNumber, ref errorMsg, true);
        }
        private Bitmap FetchGameImageFromLRLE(TGI tgi, int outfitNumber, ref string errorMsg, bool writeLog)
        {

            if (tgi.Instance == 0ul) return null;
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);
            Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            if (this.allInstances.ContainsKey(key))
            {
                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    Stream s = p.GetResource(irie);
                    if (allCCInstances.ContainsKey(key)) SaveStream(irie, s, outfitNumber >= 0 ? TroubleshootPackageOutfit : TroubleshootPackageBasic);
                    try
                    {
                        BinaryReader br = new BinaryReader(s);
                        LRLE lrle = new LRLE(br);
                        Bitmap tmp = lrle.image;
                        if (tmp.Size == currentSize) return new Bitmap(tmp);
                        else return new Bitmap(tmp, currentSize);
                    }
                    catch
                    {
                        try
                        {
                            using (RLEResource rle = new RLEResource(1, s))
                            {
                                if (rle != null && rle.AsBytes.Length > 0)
                                {
                                    using (DdsFile dds = new DdsFile())
                                    {
                                        dds.Load(rle.ToDDS(), false);
                                        Bitmap texture;
                                        Bitmap tmp = dds.Image;
                                        if (tmp.Size == currentSize) texture = new Bitmap(tmp);
                                        else texture = new Bitmap(tmp, currentSize);
                                        return texture;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (writeLog) errorMsg += "Can't read LRLE image " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1) + " : " + e.Message + Environment.NewLine;
                        }
                    }
                }
            }
            //Predicate<IResourceIndexEntry> pred2 = r => r.ResourceType == (uint)ResourceTypes.RLE2 & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            //for (int i = 0; i < gamePackages.Length; i++)
            //{
            //    Package p = gamePackages[i];
            //    IResourceIndexEntry irie = p.Find(pred2);
            //    if (irie != null)
            //    {
            //        if (isCCPackage[i] && !currentCCPackages.Any(x => x.package == p)) { currentCCPackages.Add(new CCPackage(p, Path.GetFileName(Path.GetFileName(this.allInstances[key].Item1)), outfitNumber)); }
            //        Stream s = p.GetResource(irie);
            //        try
            //        {
            //            using (RLEResource rle = new RLEResource(1, s))
            //            {
            //                if (rle != null && rle.AsBytes.Length > 0)
            //                {
            //                    using (DdsFile dds = new DdsFile())
            //                    {
            //                        dds.Load(rle.ToDDS(), false);
            //                        Bitmap texture;
            //                        Bitmap tmp = dds.Image;
            //                        if (tmp.Size == currentSize) texture = new Bitmap(tmp);
            //                        else texture = new Bitmap(tmp, currentSize);
            //                        return texture;
            //                    }
            //                }
            //            }
            //        }
            //        catch
            //        {
            //            errorMsg += "Can't read RLE image " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1) + Environment.NewLine;
            //        }
            //    }
            //}
            if (writeLog) errorMsg += "Can't find LRLE texture " + tgi.ToString() + Environment.NewLine;
            return null;
        }

        private Bitmap FetchGameTexture(ulong instance, int outfitNumber, ref string errorMsg, bool includeDDS) //Looks for LRLE, RLE2, and optionally DDS
        {
            return FetchGameTexture(new TGI((uint)ResourceTypes.LRLE, 0, instance), outfitNumber, ref errorMsg, includeDDS);
        }
        private Bitmap FetchGameTexture(TGI tgi, int outfitNumber, ref string errorMsg, bool includeDDS) //Looks for LRLE, RLE2, and optionally DDS
        {
            TGI lrleTGI = new TGI((uint)ResourceTypes.LRLE, tgi.Group, tgi.Instance);
            TGI rle2TGI = new TGI((uint)ResourceTypes.RLE2, tgi.Group, tgi.Instance);
            TGI ddsTGI = new TGI((uint)ResourceTypes.DDSuncompressed, tgi.Group, tgi.Instance);
            Bitmap image = FetchGameImageFromLRLE(lrleTGI, outfitNumber, ref errorMsg, false);
            if (image != null) return image;
            image = FetchGameImageFromRLE(rle2TGI, false, outfitNumber, ref errorMsg, false);
            if (image != null) return image;
            if (includeDDS) image = FetchGameImageFromDDS(ddsTGI, outfitNumber, ref errorMsg, false);
            if (image == null) errorMsg += "Can't find or read texture, instance: 0x" + tgi.Instance.ToString("X16") + Environment.NewLine;
            return image;
        }

        private DSTResource FetchGameDST(TGI tgi, int outfitNumber, ref string errorMsg)
        {

            if (tgi.Instance == 0ul) return null;
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);

            if (this.allInstances.ContainsKey(key))
            {
                string err = "";
                Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;

                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    Stream s = p.GetResource(irie);
                    if (this.allCCInstances.ContainsKey(key)) SaveStream(irie, s, TroubleshootPackageOutfit);
                    try
                    {
                        DSTResource dst = new DSTResource(1, s);
                        return dst;
                    }
                    catch (Exception e)
                    {
                        errorMsg += "Can't read DST " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1)+ " : " + e.Message + Environment.NewLine;
                        return null;
                    }
                }
            }
            errorMsg += "Can't find DST texture " + tgi.ToString() + Environment.NewLine;
            return null;
        }
        private Bitmap FetchGameImageFromDST(TGI tgi, int outfitNumber, ref string errorMsg)
        {

            if (tgi.Instance == 0ul) return null;
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);

            if (this.allInstances.ContainsKey(key))
            {
                string err = "";
                Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;

                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    Stream s = p.GetResource(irie);
                    if (this.allCCInstances.ContainsKey(key)) SaveStream(irie, s, TroubleshootPackageOutfit);
                    try
                    {
                        using (DSTResource dst = new DSTResource(1, s))
                        {
                            if (dst != null)
                            {
                                using (DdsFile dds = new DdsFile())
                                {
                                    dds.Load(dst.ToDDS(), false);
                                    return new Bitmap(dds.Image);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        errorMsg += "Can't read DST image " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1) + " : " + e.Message + Environment.NewLine;
                        return null;
                    }
                }
            }
            errorMsg += "Can't find DST texture " + tgi.ToString() + Environment.NewLine;
            return null;
        }

        private DdsFile FetchGameDDS(TGI tgi, int outfitNumber, ref string errorMsg)
        {

            if (tgi.Instance == 0ul) return null;
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);
            if (this.allInstances.ContainsKey(key))
            {
                string err = "";
                Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;


                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    Stream s = p.GetResource(irie);
                    if (this.allCCInstances.ContainsKey(key)) SaveStream(irie, s, TroubleshootPackageOutfit);
                    try
                    {
                        DdsFile dds = new DdsFile();
                        dds.Load(s, false);
                        return dds;
                    }
                    catch (Exception e)
                    {
                        errorMsg += "Can't read DDS " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1)  + " : " + e.Message + Environment.NewLine;
                        return null;
                    }
                }
            }
            errorMsg += "Can't find DDS texture " + tgi.ToString() + Environment.NewLine;
            return null;
        }
        private Bitmap FetchGameImageFromDDS(TGI tgi, int outfitNumber, ref string errorMsg, bool writeLog)
        {

            if (tgi.Instance == 0ul) return null;
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);

            if (this.allInstances.ContainsKey(key))
            {
                string err = "";
                Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;

                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);

                if (irie != null)
                {
                    Stream s = p.GetResource(irie);
                    if (this.allCCInstances.ContainsKey(key)) SaveStream(irie, s, outfitNumber >= 0 ? TroubleshootPackageOutfit : TroubleshootPackageBasic);
                    using (DdsFile dds = new DdsFile())
                    {
                        try
                        {
                            dds.Load(s, false);
                            Bitmap texture;
                            if (dds.Size == currentSize) texture = new Bitmap(dds.Image);
                            else texture = new Bitmap(dds.Image, currentSize);
                            return texture;
                        }
                        catch (Exception e)
                        {
                            if (writeLog)
                                errorMsg += "Can't read DDS image " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1) + " : " + e.Message + Environment.NewLine;
                            return null;
                        }
                    }
                }
            }
            if (writeLog) errorMsg += "Can't find DDS texture " + tgi.ToString() + Environment.NewLine;
            return null;
        }
        private TONE FetchGameTONE(TGI tgi, out string packageName, ref string errorMsg)
        {

            if (tgi.Instance == 0ul) { packageName = ""; return null; }
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);
            if (this.allInstances.ContainsKey(key))
            {
                string err = "";
                Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;

                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        if (this.allCCInstances.ContainsKey(key)) SaveStream(irie, br, TroubleshootPackageBasic);
                        try
                        {
                            TONE tone = new TONE(br);
                            packageName = Path.GetFileName(allInstances[key].Item1);
                            return tone;
                        }
                        catch (Exception e)
                        {
                            errorMsg += "Can't read TONE " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1) + " : " + e.Message + Environment.NewLine;
                            packageName = Path.GetFileName(allInstances[key].Item1);
                            return null;
                        }
                    }
                }
            }
            errorMsg += "Can't find skin TONE " + tgi.ToString() + Environment.NewLine;
            packageName = "";
            return null;
        }
        private PeltLayer FetchGamePeltLayer(TGI tgi, ref string errorMsg)
        {

            if (tgi.Instance == 0ul) return null;
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);

            if (this.allInstances.ContainsKey(key))
            {
                string err = "";
                Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;

                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        if(this.allCCInstances.ContainsKey(key)) SaveStream(irie, br, TroubleshootPackageBasic);
                        try
                        {
                            PeltLayer pelt = new PeltLayer(br);
                            return pelt;
                        }
                        catch (Exception e)
                        {
                            errorMsg += "Can't read Pelt " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1) + " : " + e.Message + Environment.NewLine;
                            return null;
                        }
                    }
                }
            }
            errorMsg += "Can't find cat/dog Pelt Layer " + tgi.ToString() + Environment.NewLine;
            return null;
        }
        private RIG FetchGameRig(TGI tgi, ref string errorMsg)
        {

            if (tgi.Instance == 0ul) return null;
            (uint ResourceType, uint ResourceGroup, ulong Instance) key = (tgi.Type, tgi.Group, tgi.Instance);

            Predicate<IResourceIndexEntry> pred = r => (r.ResourceType == tgi.Type) & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            if (this.allInstances.ContainsKey(key))
            {
                string err = "";

                Package p = this.allInstances[key].Item2;
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        if (this.allCCInstances.ContainsKey(key))
                            SaveStream(irie, br, TroubleshootPackageBasic);
                        try
                        {
                            RIG rig = new RIG(br);
                            return rig;
                        }
                        catch (Exception e)
                        {
                            errorMsg += "Can't read Rig " + tgi.ToString() + ", Package: " + Path.GetFileName(this.allInstances[key].Item1) + " : " + e.Message + Environment.NewLine;
                            return null;
                        }
                    }

                }
            }
            errorMsg += "Can't find Rig " + tgi.ToString() + Environment.NewLine;
            return null;
        }
        
    }
}
