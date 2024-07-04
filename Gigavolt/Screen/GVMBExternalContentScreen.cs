using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Engine.Media;

namespace Game {
    public class GVMBExternalContentScreen : Screen {
        public Screen m_previousScreen;

        public readonly LabelWidget m_directoryLabel;

        public readonly ListPanelWidget m_directoryList;

        public readonly ButtonWidget m_upDirectoryButton;
        public readonly ButtonWidget m_exportButton;
        public readonly ButtonWidget m_actionButton;

        public string m_path;

        public bool m_listDirty;

        public readonly IExternalContentProvider m_externalContentProvider = ExternalContentManager.DefaultProvider;

        BaseEditGVMemoryBankDialog m_dialog;
        GVArrayData m_arrayData;

        public GVMBExternalContentScreen() {
            XElement node = ContentManager.Get<XElement>("Screens/GVMBExternalContentScreen");
            LoadContents(this, node);
            m_directoryLabel = Children.Find<LabelWidget>("TopBar.Label");
            m_directoryList = Children.Find<ListPanelWidget>("DirectoryList");
            m_upDirectoryButton = Children.Find<ButtonWidget>("UpDirectory");
            m_exportButton = Children.Find<ButtonWidget>("Export");
            m_actionButton = Children.Find<ButtonWidget>("Action");
            m_directoryList.ItemWidgetFactory = delegate(object item) {
                ExternalContentEntry externalContentEntry2 = (ExternalContentEntry)item;
                XElement node2 = ContentManager.Get<XElement>("Widgets/ExternalContentItem");
                ContainerWidget containerWidget = (ContainerWidget)LoadWidget(this, node2, null);
                string fileName = Storage.GetFileName(externalContentEntry2.Path);
                string typeDescription;
                Subtexture typeIcon;
                string extension = Storage.GetExtension(fileName).ToLower();
                switch (extension) {
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
                string text2 = externalContentEntry2.Type != ExternalContentType.Directory ? $"{typeDescription} | {DataSizeFormatter.Format(externalContentEntry2.Size)} | {externalContentEntry2.Time:dd-MMM-yyyy HH:mm}" : ExternalContentManager.GetEntryTypeDescription(externalContentEntry2.Type);
                containerWidget.Children.Find<RectangleWidget>("ExternalContentItem.Icon").Subtexture = typeIcon;
                containerWidget.Children.Find<LabelWidget>("ExternalContentItem.Text").Text = fileName;
                containerWidget.Children.Find<LabelWidget>("ExternalContentItem.Details").Text = text2;
                return containerWidget;
            };
            m_directoryList.ItemClicked += delegate(object item) {
                if (m_directoryList.SelectedItem == item) {
                    if (item is ExternalContentEntry externalContentEntry
                        && externalContentEntry.Type == ExternalContentType.Directory) {
                        SetPath(externalContentEntry.Path);
                    }
                }
            };
        }

        public override void Enter(object[] parameters) {
            if (m_previousScreen == null) {
                m_previousScreen = ScreensManager.PreviousScreen;
            }
            m_dialog = (BaseEditGVMemoryBankDialog)parameters[0];
            m_arrayData = m_dialog.GetArrayData();
            m_directoryList.ClearItems();
            SetPath(null);
            m_listDirty = true;
        }

        public override void Update() {
            if (m_listDirty) {
                m_listDirty = false;
                UpdateList();
            }
            ExternalContentEntry externalContentEntry = null;
            if (m_directoryList.SelectedIndex.HasValue) {
                externalContentEntry = m_directoryList.Items[m_directoryList.SelectedIndex.Value] as ExternalContentEntry;
            }
            if (externalContentEntry != null) {
                m_actionButton.IsVisible = true;
                if (externalContentEntry.Type == ExternalContentType.Directory) {
                    m_actionButton.Text = LanguageControl.Get(GetType().Name, 1);
                    m_actionButton.IsEnabled = true;
                }
                else {
                    m_actionButton.Text = LanguageControl.Get(GetType().Name, 2);
                    m_actionButton.IsEnabled = true;
                }
            }
            else {
                m_actionButton.IsVisible = false;
            }
            m_directoryLabel.Text = string.Format(LanguageControl.Get(GetType().Name, 3), m_path);
            m_upDirectoryButton.IsEnabled = m_externalContentProvider.IsLoggedIn && m_path != "/";
            if (m_upDirectoryButton.IsClicked) {
                string directoryName = Storage.GetDirectoryName(m_path);
                SetPath(directoryName);
            }
            if (m_exportButton.IsClicked) {
                MemoryStream stream = m_arrayData.GetStream();
                if (stream != null) {
                    ExportFile($"{m_path}/{m_arrayData.m_ID:X}{m_arrayData.ExportExtension}", stream);
                }
            }
            if (m_actionButton.IsClicked
                && externalContentEntry != null) {
                if (externalContentEntry.Type == ExternalContentType.Directory) {
                    SetPath(externalContentEntry.Path);
                }
                else {
                    DownloadEntry(externalContentEntry);
                }
            }
            if (Input.Back
                || Input.Cancel
                || Children.Find<ButtonWidget>("TopBar.Back").IsClicked) {
                ScreensManager.SwitchScreen(m_previousScreen);
                m_previousScreen = null;
            }
        }

        public void SetPath(string path) {
            if (string.IsNullOrEmpty(path)) {
                switch (VersionsManager.Platform) {
                    case Platform.Desktop:
                        path = AppDomain.CurrentDomain.BaseDirectory;
                        break;
                    case Platform.Android:
                        path = Storage.GetSystemPath("android:SurvivalCraft2.3");
                        break;
                    default: throw new Exception("Unsupported platform");
                }
            }
            path = path.Replace("\\", "/");
            if (path != m_path) {
                m_path = path;
                m_listDirty = true;
            }
        }

        public void UpdateList() {
            m_directoryList.ClearItems();
            if (m_externalContentProvider != null
                && m_externalContentProvider.IsLoggedIn) {
                CancellableBusyDialog busyDialog = new(LanguageControl.Get(GetType().Name, 9), false);
                DialogsManager.ShowDialog(null, busyDialog);
                m_externalContentProvider.List(
                    m_path,
                    busyDialog.Progress,
                    delegate(ExternalContentEntry entry) {
                        DialogsManager.HideDialog(busyDialog);
                        List<ExternalContentEntry> list = new(entry.ChildEntries.Where(EntryFilter).Take(1000));
                        m_directoryList.ClearItems();
                        list.Sort(
                            delegate(ExternalContentEntry e1, ExternalContentEntry e2) {
                                if (e1.Type == ExternalContentType.Directory
                                    && e2.Type != ExternalContentType.Directory) {
                                    return -1;
                                }
                                return e1.Type != ExternalContentType.Directory && e2.Type == ExternalContentType.Directory ? 1 : string.CompareOrdinal(e1.Path, e2.Path);
                            }
                        );
                        foreach (ExternalContentEntry item in list) {
                            m_directoryList.AddItem(item);
                        }
                    },
                    delegate(Exception error) {
                        DialogsManager.HideDialog(busyDialog);
                        DialogsManager.ShowDialog(
                            null,
                            new MessageDialog(
                                LanguageControl.Error,
                                error.ToString(),
                                LanguageControl.Ok,
                                null,
                                null
                            )
                        );
                    }
                );
            }
        }

        public void ExportFile(string path, MemoryStream stream) {
            CancellableBusyDialog busyDialog = new(LanguageControl.Get(GetType().Name, 13), false);
            DialogsManager.ShowDialog(null, busyDialog);
            try {
                FileStream fileStream = File.OpenWrite(path);
                stream.WriteTo(fileStream);
                fileStream.Flush(true);
                fileStream.Close();
                stream.Close();
                DialogsManager.HideDialog(busyDialog);
                DialogsManager.ShowDialog(
                    null,
                    new MessageDialog(
                        LanguageControl.Get(GetType().Name, 14),
                        path,
                        LanguageControl.Ok,
                        null,
                        null
                    )
                );
            }
            catch (Exception e) {
                DialogsManager.HideDialog(busyDialog);
                DialogsManager.ShowDialog(
                    null,
                    new MessageDialog(
                        LanguageControl.Error,
                        e.ToString(),
                        LanguageControl.Ok,
                        null,
                        null
                    )
                );
            }
        }

        public void DownloadEntry(ExternalContentEntry entry) {
            CancellableBusyDialog busyDialog = new(LanguageControl.Get(GetType().Name, 10), false);
            DialogsManager.ShowDialog(null, busyDialog);
            m_externalContentProvider.Download(
                entry.Path,
                busyDialog.Progress,
                delegate(Stream stream) {
                    busyDialog.LargeMessage = LanguageControl.Get(GetType().Name, 12);
                    try {
                        string extension = Storage.GetExtension(entry.Path).ToLower();
                        string result;
                        switch (extension) {
                            case ".png":
                                Image image = Image.Load(stream, ImageFileFormat.Png);
                                m_arrayData.Image2Data(image);
                                result = string.Format(LanguageControl.Get(GetType().Name, 6), entry.Path, image.Width, image.Height);
                                break;
                            case ".wav":
                                SoundData soundData = Wav.Load(stream);
                                if (soundData.ChannelsCount != 2) {
                                    throw new Exception(string.Format(LanguageControl.Get(GetType().Name, 7), entry.Path));
                                }
                                m_arrayData.Shorts2Data(soundData.Data);
                                result = string.Format(
                                    LanguageControl.Get(GetType().Name, 8),
                                    entry.Path,
                                    soundData.SamplingFrequency,
                                    soundData.Data.Length / 2,
                                    (soundData.Data.Length / (double)soundData.SamplingFrequency / 2).ToString("F2", null)
                                );
                                break;
                            default:
                                long length = stream.Length;
                                string desc = m_arrayData.Stream2Data(stream, extension);
                                result = string.Format(LanguageControl.Get(GetType().Name, 11), entry.Path, length, desc.Length > 0 ? desc : LanguageControl.Get(GetType().Name, 17));
                                break;
                        }
                        m_arrayData.SaveString();
                        m_dialog.UpdateFromData();
                        m_dialog.Dismiss(true, false);
                        DialogsManager.HideDialog(busyDialog);
                        DialogsManager.ShowDialog(
                            null,
                            new MessageDialog(
                                LanguageControl.Get(GetType().Name, 16),
                                result,
                                LanguageControl.Ok,
                                null,
                                null
                            )
                        );
                    }
                    catch (Exception e) {
                        DialogsManager.HideDialog(busyDialog);
                        DialogsManager.ShowDialog(
                            null,
                            new MessageDialog(
                                LanguageControl.Error,
                                e.ToString(),
                                LanguageControl.Ok,
                                null,
                                null
                            )
                        );
                    }
                    stream.Close();
                },
                delegate(Exception error) {
                    DialogsManager.HideDialog(busyDialog);
                    DialogsManager.ShowDialog(
                        null,
                        new MessageDialog(
                            LanguageControl.Error,
                            error.ToString(),
                            LanguageControl.Ok,
                            null,
                            null
                        )
                    );
                }
            );
        }

        public static bool EntryFilter(ExternalContentEntry entry) => true;
    }
}