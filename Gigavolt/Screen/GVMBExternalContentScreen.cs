using Engine;
using Engine.Graphics;
using Engine.Media;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;

namespace Game
{
    public class GVMBExternalContentScreen : Screen
    {
        public Screen m_previousScreen;

        public LabelWidget m_directoryLabel;

        public ListPanelWidget m_directoryList;

        public ButtonWidget m_upDirectoryButton;
        public ButtonWidget m_exportButtion;
        public ButtonWidget m_actionButton;

        public string m_path;

        public bool m_listDirty;

        public IExternalContentProvider m_externalContentProvider = ExternalContentManager.DefaultProvider;

        EditGVMemoryBankDialog m_dialog;

        public GVMBExternalContentScreen()
        {
            XElement node = ContentManager.Get<XElement>("Screens/GVMBExternalContentScreen");
            LoadContents(this, node);
            m_directoryLabel = Children.Find<LabelWidget>("TopBar.Label");
            m_directoryList = Children.Find<ListPanelWidget>("DirectoryList");
            m_upDirectoryButton = Children.Find<ButtonWidget>("UpDirectory");
            m_exportButtion = Children.Find<ButtonWidget>("Export");
            m_actionButton = Children.Find<ButtonWidget>("Action");
            m_directoryList.ItemWidgetFactory = delegate (object item)
            {
                var externalContentEntry2 = (ExternalContentEntry)item;
                XElement node2 = ContentManager.Get<XElement>("Widgets/ExternalContentItem");
                var containerWidget = (ContainerWidget)LoadWidget(this, node2, null);
                string fileName = Storage.GetFileName(externalContentEntry2.Path);
                string typeDescription;
                Subtexture typeIcon;
                string extension = Storage.GetExtension(fileName).ToLower();
                switch (extension)
                {
                    case ".png":
                        typeDescription = LanguageControl.Get(GetType().Name, 4);
                        typeIcon = ExternalContentManager.GetEntryTypeIcon(externalContentEntry2.Type);
                        break;
                    case ".wav":
                        typeDescription = LanguageControl.Get(GetType().Name, 5);
                        typeIcon = new Subtexture(ContentManager.Get<Texture2D>("Textures/SoundParticle"), new Vector2(0.5f), new Vector2(1f));
                        break;
                    default:
                        typeDescription = ExternalContentManager.GetEntryTypeDescription(externalContentEntry2.Type);
                        typeIcon = ExternalContentManager.GetEntryTypeIcon(externalContentEntry2.Type);
                        break;
                }
                string text2 = (externalContentEntry2.Type != ExternalContentType.Directory) ? $"{typeDescription} | {DataSizeFormatter.Format(externalContentEntry2.Size)} | {externalContentEntry2.Time:dd-MMM-yyyy HH:mm}" : ExternalContentManager.GetEntryTypeDescription(externalContentEntry2.Type);
                containerWidget.Children.Find<RectangleWidget>("ExternalContentItem.Icon").Subtexture = typeIcon;
                containerWidget.Children.Find<LabelWidget>("ExternalContentItem.Text").Text = fileName;
                containerWidget.Children.Find<LabelWidget>("ExternalContentItem.Details").Text = text2;
                return containerWidget;
            };
            m_directoryList.ItemClicked += delegate (object item)
            {
                if (m_directoryList.SelectedItem == item)
                {
                    var externalContentEntry = item as ExternalContentEntry;
                    if (externalContentEntry != null && externalContentEntry.Type == ExternalContentType.Directory)
                    {
                        SetPath(externalContentEntry.Path);
                    }
                }
            };
        }

        public override void Enter(object[] parameters)
        {
            if (m_previousScreen == null)
            {
                m_previousScreen = ScreensManager.PreviousScreen;
            }
            m_dialog = (EditGVMemoryBankDialog)parameters[0];
            m_directoryList.ClearItems();
            SetPath(null);
            m_listDirty = true;
        }

        public override void Update()
        {
            if (m_listDirty)
            {
                m_listDirty = false;
                UpdateList();
            }
            ExternalContentEntry externalContentEntry = null;
            if (m_directoryList.SelectedIndex.HasValue)
            {
                externalContentEntry = (m_directoryList.Items[m_directoryList.SelectedIndex.Value] as ExternalContentEntry);
            }
            if (externalContentEntry != null)
            {
                m_actionButton.IsVisible = true;
                if (externalContentEntry.Type == ExternalContentType.Directory)
                {
                    m_actionButton.Text = LanguageControl.Get(GetType().Name, 1);
                    m_actionButton.IsEnabled = true;
                }
                else
                {
                    m_actionButton.Text = LanguageControl.Get(GetType().Name, 2);
                    m_actionButton.IsEnabled = true;
                }
            }
            else
            {
                m_actionButton.IsVisible = false;
            }
            m_directoryLabel.Text = string.Format(LanguageControl.Get(GetType().Name, 3), m_path);
            m_upDirectoryButton.IsEnabled = (m_externalContentProvider.IsLoggedIn && m_path != "/");
            if (m_upDirectoryButton.IsClicked)
            {
                string directoryName = Storage.GetDirectoryName(m_path);
                SetPath(directoryName);
            }
            if(m_exportButtion.IsClicked)
            {
                ExportImage($"{m_path}/{m_dialog.m_memoryBankData.m_ID.ToString("X", null)}.png", m_dialog.m_memoryBankData.Data);
            }
            if (m_actionButton.IsClicked && externalContentEntry != null)
            {
                if (externalContentEntry.Type == ExternalContentType.Directory)
                {
                    SetPath(externalContentEntry.Path);
                }
                else
                {
                    DownloadEntry(externalContentEntry);
                }
            }
            if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
            {
                ScreensManager.SwitchScreen(m_previousScreen);
                m_previousScreen = null;
            }
        }

        public void SetPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = DiskExternalContentProvider.LocalPath;
            }
            path = path.Replace("\\", "/");
            if (path != m_path)
            {
                m_path = path;
                m_listDirty = true;
            }
        }

        public void UpdateList()
        {
            m_directoryList.ClearItems();
            if (m_externalContentProvider != null && m_externalContentProvider.IsLoggedIn)
            {
                var busyDialog = new CancellableBusyDialog(LanguageControl.Get(GetType().Name, 9), autoHideOnCancel: false);
                DialogsManager.ShowDialog(null, busyDialog);
                m_externalContentProvider.List(m_path, busyDialog.Progress, delegate (ExternalContentEntry entry)
                {
                    DialogsManager.HideDialog(busyDialog);
                    var list = new List<ExternalContentEntry>(entry.ChildEntries.Where((ExternalContentEntry e) => EntryFilter(e)).Take(1000));
                    m_directoryList.ClearItems();
                    list.Sort(delegate (ExternalContentEntry e1, ExternalContentEntry e2)
                    {
                        if (e1.Type == ExternalContentType.Directory && e2.Type != ExternalContentType.Directory)
                        {
                            return -1;
                        }
                        return (e1.Type != ExternalContentType.Directory && e2.Type == ExternalContentType.Directory) ? 1 : string.Compare(e1.Path, e2.Path);
                    });
                    foreach (ExternalContentEntry item in list)
                    {
                        m_directoryList.AddItem(item);
                    }
                }, delegate (Exception error)
                {
                    DialogsManager.HideDialog(busyDialog);
                    DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Error, error.ToString(), LanguageControl.Ok, null, null));
                });
            }
        }
        public void ExportImage(string path, Image image)
        {
            var busyDialog = new CancellableBusyDialog(LanguageControl.Get(GetType().Name, 13), autoHideOnCancel: false);
            DialogsManager.ShowDialog(null, busyDialog);
            try
            {
                FileStream fileStream = null;
                fileStream = File.OpenWrite(path);
                Image.Save(image, fileStream, ImageFileFormat.Png, true);
                fileStream.Close();
                DialogsManager.HideDialog(busyDialog);
                DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(GetType().Name, 14), path, LanguageControl.Ok, null, null));
            }
            catch (Exception e)
            {
                DialogsManager.HideDialog(busyDialog);
                DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Error, e.ToString(), LanguageControl.Ok, null, null));
            }
        }
        public void DownloadEntry(ExternalContentEntry entry)
        {
            var busyDialog = new CancellableBusyDialog(LanguageControl.Get(GetType().Name, 10), autoHideOnCancel: false);
            DialogsManager.ShowDialog(null, busyDialog);
            m_externalContentProvider.Download(entry.Path, busyDialog.Progress, delegate (Stream stream)
            {
                busyDialog.LargeMessage = LanguageControl.Get(GetType().Name, 12);
                GVMemoryBankData GVMBData = m_dialog.m_memoryBankData;
                try
                {
                    string extension = Storage.GetExtension(entry.Path).ToLower();
                    string result;
                    switch (extension)
                    {
                        case ".png":
                            GVMBData.Data = Image.Load(stream, ImageFileFormat.Png);
                            result = string.Format(LanguageControl.Get(GetType().Name, 6), entry.Path, GVMBData.Data.Width, GVMBData.Data.Height);
                            break;
                        case ".wav":
                            var soundData = Wav.Load(stream);
                            if (soundData.ChannelsCount != 2)
                            {
                                throw new Exception(string.Format(LanguageControl.Get(GetType().Name, 7), entry.Path));
                            }
                            GVMBData.Data = Shorts2Image(soundData.Data);
                            result = string.Format(LanguageControl.Get(GetType().Name, 8), entry.Path, soundData.SamplingFrequency, soundData.Data.Length / 2, (double)soundData.Data.Length / (double)soundData.SamplingFrequency / 2); 
                            break;
                        default:
                            GVMBData.Data = Stream2Image(stream);
                            result = string.Format(LanguageControl.Get(GetType().Name, 11), entry.Path, stream.Length);
                            break;
                    }
                    Image.Save(GVMBData.Data, $"{GVMBData.m_worldDirectory}/GVMB/{GVMBData.m_ID.ToString("X", null)}.png", ImageFileFormat.Png, true);
                    m_dialog.UpdateFromData();
                    DialogsManager.HideDialog(busyDialog);
                    DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(GetType().Name, 16), result, LanguageControl.Ok, null, null));
                }
                catch (Exception e)
                {
                    DialogsManager.HideDialog(busyDialog);
                    DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Error, e.ToString(), LanguageControl.Ok, null, null));
                }
                stream.Close();
            }, delegate (Exception error)
            {
                DialogsManager.HideDialog(busyDialog);
                DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Error, error.ToString(), LanguageControl.Ok, null, null));
            });
        }

        public static bool EntryFilter(ExternalContentEntry entry)
        {
            return true;
        }
        public static Image Shorts2Image(short[] shorts)
        {
            int width = (int)Math.Ceiling(Math.Sqrt(shorts.Length / 2 + 1));
            Image image = new Image(width, width);
            for (int i = 0; i < image.Pixels.Length; i++)
            {
                if (i * 2 >= shorts.Length)
                {
                    break;
                }
                else if (i * 2 == shorts.Length - 1)
                {
                    image.Pixels[i] = new Color(((uint)((ushort)shorts[i * 2])) << 16);
                }
                else
                {
                    image.Pixels[i] = new Color(((uint)((ushort)shorts[i * 2 + 1])) | (((uint)((ushort)shorts[i * 2])) << 16));
                }
            }
            return image;
        }
        public static Image Stream2Image(Stream stream)
        {
            int width = (int)Math.Ceiling(Math.Sqrt(stream.Length / 4 + 1));
            Image image = new Image(width, width);
            byte[] fourBytes = new byte[4];
            for (int i = 0; i < stream.Length / 4 + 1; i++)
            {
                if (stream.Read(fourBytes, 0, 4) > 0)
                {
                    Color color = new Color(((uint)fourBytes[3]) | (((uint)fourBytes[2]) << 8) | (((uint)fourBytes[1]) << 16) | (((uint)fourBytes[0]) << 24));
                    image.SetPixel(i % width, i / width, color);
                }
            }
            return image;
        }
    }
}