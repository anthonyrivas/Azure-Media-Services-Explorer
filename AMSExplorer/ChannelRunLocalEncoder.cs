﻿//----------------------------------------------------------------------- 
// <copyright file="ChannelRunLocalEncoder.cs" company="Microsoft">Copyright (c) Microsoft Corporation. All rights reserved.</copyright> 
// <license>
// Azure Media Services Explorer Ver. 3.1
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
//  
// http://www.apache.org/licenses/LICENSE-2.0 
//  
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License. 
// </license> 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using System.Configuration;
using Microsoft.WindowsAzure.MediaServices.Client;
using System.IO;

namespace AMSExplorer
{
    public partial class ChannelRunLocalEncoder : Form
    {
        CloudMediaContext _context;
        IList<IChannel> _channels;

        public readonly IList<LocalEncoder> Encoders = new List<LocalEncoder> {
            // ffmpg list devices
            new LocalEncoder() {Name="ffmpeg - List devices",  Folder=@"C:\temp\ffmpeg\bin\",InstallURL=new Uri("https://www.ffmpeg.org/download.html"), CanBeRunLocally=true, Command= @"ffmpeg.exe -list_devices true -f dshow -i dummy"}, 
            // ffmpeg RTMP
            new LocalEncoder() {Name="ffmpeg (RTMP)", Folder=@"C:\temp\ffmpeg\bin\", InstallURL=new Uri("https://www.ffmpeg.org/download.html"), CanBeRunLocally=true, Command= @"ffmpeg.exe -y -loglevel verbose -f dshow -video_size 1280x720 -r 30 -i video=""%videodevicename%"":audio=""%audiodevicename%"" -strict -2 -c:v libx264 -preset faster -g 60 -keyint_min 60 -vsync cfr -b:v %videobitrate%k -maxrate %videobitrate%k -minrate %videobitrate%k -c:v libx264 -c:a aac -b:a %audiobitrate%k -ar 44100 -f flv %output%/MyStream1"}, 
            // ffmpeg RTP
            new LocalEncoder() {Name="ffmpeg (RTP MPEG-TS)", Folder=@"C:\temp\ffmpeg\bin\",InstallURL=new Uri("https://www.ffmpeg.org/download.html"), CanBeRunLocally=true, Command= @"ffmpeg -y -f dshow -video_size 1280x720 -r 30 -i video=""%videodevicename%"":audio=""%audiodevicename%"" -c:v libx264 -preset ultrafast -bf 0 -g 60  -vsync cfr -b:v %videobitrate%k -minrate %videobitrate%k -maxrate %videobitrate%k -bufsize %videobitrate%k -strict -2 -c:a aac -ac 2 -ar 44100 -b:a %audiobitrate%k -f mpegts %output%"}, 
            // VLC
            new LocalEncoder() {Name="VLC (RTMP) 32 bit", Folder="%programfiles32%\\VideoLAN\\VLC\\",InstallURL=new Uri("http://www.videolan.org/vlc/"), CanBeRunLocally=true, Command= @"vlc.exe dshow:// :dshow-vdev=""%videodevicename%"" :dshow-adev=""%audiodevicename%"" :dshow-size=320 :live-caching=3000  :sout=""#transcode{vcodec=h264,vb=%videobitrate%,scale=1,fps=30,venc=x264{keyint=60,preset=veryfast,level=-1,profile=baseline,cabac,slices=8,qcomp=0.4,vbv-maxrate=%videobitrate%,vbv-bufsize=400},acodec=aac,aenc=ffmpeg{strict=-2,b:a=%audiobitrate%k,ac=2,ar=44100}}:std{access=rtmp,mux=ffmpeg{mux=flv},dst=%output%/MyStream1}"" :sout-keep :sout-all :sout-mux-caching=5000"} ,
            new LocalEncoder() {Name="VLC (RTMP) 64 bit", Folder="%programfiles64%\\VideoLAN\\VLC\\",InstallURL=new Uri("http://www.videolan.org/vlc/"), CanBeRunLocally=true, Command= @"vlc.exe dshow:// :dshow-vdev=""%videodevicename%"" :dshow-adev=""%audiodevicename%"" :dshow-size=320 :live-caching=3000  :sout=""#transcode{vcodec=h264,vb=%videobitrate%,scale=1,fps=30,venc=x264{keyint=60,preset=veryfast,level=-1,profile=baseline,cabac,slices=8,qcomp=0.4,vbv-maxrate=%videobitrate%,vbv-bufsize=400},acodec=aac,aenc=ffmpeg{strict=-2,b:a=%audiobitrate%k,ac=2,ar=44100}}:std{access=rtmp,mux=ffmpeg{mux=flv},dst=%output%/MyStream1}"" :sout-keep :sout-all :sout-mux-caching=5000"} ,
            // Azure Media Capture
            new LocalEncoder() {Name="Azure Media Services Capture (Windows Phone)", Folder="",InstallURL=new Uri("http://www.windowsphone.com/s?appid=12dc1fcc-c5bd-4af0-afd8-f30745f94b84"), CanBeRunLocally=false, Command= @"Install the app http://www.windowsphone.com/s?appid=12dc1fcc-c5bd-4af0-afd8-f30745f94b84 on Windows Phone and enter the input URL %output%"} 
      
        };


        public ChannelRunLocalEncoder(CloudMediaContext context, IList<IChannel> channels)
        {
            InitializeComponent();
            this.Icon = Bitmaps.Azure_Explorer_ico;
            _context = context;
            _channels = channels;
        }


        private void ChannelRunLocalEncoder_Load(object sender, EventArgs e)
        {
            labelChannel.Text = string.Format(labelChannel.Text, _channels.FirstOrDefault().Name, _channels.FirstOrDefault().Input.StreamingProtocol.ToString());
            labelURL.Text = string.Format(labelURL.Text, _channels.FirstOrDefault().Input.Endpoints.FirstOrDefault().Url.ToString());

            foreach (var encoder in Encoders)
            {
                comboBoxEncoder.Items.Add(encoder.Name);
            }
            comboBoxEncoder.SelectedIndex = 0;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo proc = new System.Diagnostics.ProcessStartInfo();
                proc.WorkingDirectory = textBoxFolder.Text;
                proc.FileName = @"cmd.exe";
                proc.Arguments = "/K " + textBoxCommand.Text;
                System.Diagnostics.Process.Start(proc);
            }
            catch (Exception ex)
            {

            }
        }

        private void EncoderSettings_Changed(object sender, EventArgs e)
        {
            BuildArguments();
        }

        private void BuildArguments(bool buildfolder = false)
        {
            LocalEncoder SelectedEncoder = Encoders.Where(m => m.Name == comboBoxEncoder.Text).FirstOrDefault();
            textBoxCommand.Text = SelectedEncoder.Command.Replace("%output%", _channels.FirstOrDefault().Input.Endpoints.FirstOrDefault().Url.AbsoluteUri)
                .Replace("%audiodevicename%", textBoxAudioDeviceName.Text.Trim())
                .Replace("%videodevicename%", textBoxVideoDeviceName.Text.Trim())
                .Replace("%audiobitrate%", textBoxAudioBitRate.Text.Trim())
                .Replace("%videobitrate%", textBoxVideoBitRate.Text.Trim());

            if (buildfolder)
            {
                textBoxFolder.Text = SelectedEncoder.Folder
                    .Replace("%programfiles32%", Environment.GetFolderPath(Environment.Is64BitOperatingSystem ? Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles))
                    .Replace("%programfiles64%", System.Environment.ExpandEnvironmentVariables("%systemdrive%\\Program Files"));
                buttonOk.Enabled = SelectedEncoder.CanBeRunLocally;
                linkLabelInstall.Links.Clear();
                linkLabelInstall.Links.Add(new LinkLabel.Link(0, linkLabelInstall.Text.Length, SelectedEncoder.InstallURL.ToString()));
            }
        }

        private void comboBoxEncoder_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildArguments(true);
        }

        private void linkLabelInstall_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData as string);
        }
    }
}