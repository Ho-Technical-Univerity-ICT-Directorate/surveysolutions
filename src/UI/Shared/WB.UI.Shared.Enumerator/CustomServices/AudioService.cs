﻿using System;
using System.Diagnostics;
using Android.Media;
using Android.Webkit;
using Java.IO;
using Java.Lang;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;
using Exception = System.Exception;
using Math = System.Math;
using Stream = System.IO.Stream;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class AudioService : IAudioService
    {
        private class AudioRecorderInfoLisener : Java.Lang.Object, MediaRecorder.IOnInfoListener
        {
            public event EventHandler OnMaxDurationReached;
            public void OnInfo(MediaRecorder mr, MediaRecorderInfo what, int extra)
            {
                if(what == MediaRecorderInfo.MaxDurationReached)
                    this.OnMaxDurationReached?.Invoke(this, EventArgs.Empty);
            }
        }

        private const int MaxDuration = 3 * 60 * 1000;
        private const double MaxReportableAmp = 32767f;
        private const double MaxReportableDb = 90.3087f;
        private readonly string audioFileName = $"audio.{AudioFileExtension}";
        private const string AudioFileExtension = "m4a";
        
        private readonly Stopwatch duration = new Stopwatch();

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string pathToAudioFile;

        private MediaRecorder recorder;
        private AudioRecorderInfoLisener audioRecorderInfoLisener;


        public AudioService(string pathToAudioDirectory, IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.pathToAudioFile = this.fileSystemAccessor.CombinePath(pathToAudioDirectory, audioFileName);
        }

        public event EventHandler OnMaxDurationReached;

        public void Start()
        {
            if (this.recorder != null) return;

            this.recorder = new MediaRecorder();

            if (this.fileSystemAccessor.IsFileExists(this.pathToAudioFile))
                this.fileSystemAccessor.DeleteFile(this.pathToAudioFile);
            
            this.recorder.SetAudioSource(AudioSource.Mic);
            this.recorder.SetOutputFormat(OutputFormat.Mpeg4);
            this.recorder.SetAudioEncoder(AudioEncoder.Aac);
            this.recorder.SetAudioChannels(1);
            this.recorder.SetAudioSamplingRate(44100);
            this.recorder.SetAudioEncodingBitRate(64000);
            this.recorder.SetOutputFile(this.pathToAudioFile);
            this.recorder.SetMaxDuration(MaxDuration);

            this.audioRecorderInfoLisener = new AudioRecorderInfoLisener();
            this.audioRecorderInfoLisener.OnMaxDurationReached += this.AudioRecorderInfoLisener_OnMaxDurationReached;
            this.recorder.SetOnInfoListener(audioRecorderInfoLisener);

            try
            {
                this.recorder.Prepare();
                this.recorder.Start();
                this.duration.Restart();
            }
            catch (Exception ex) when (ex.GetSelfOrInnerAs<IOException>() != null)
            {
                this.ReleaseAudioRecorder();
                throw new AudioException(AudioExceptionType.Io, "Could not write audio file", ex);
            }
            catch (Exception ex) when (ex.GetSelfOrInnerAs<IllegalStateException>() != null)
            {
                this.ReleaseAudioRecorder();
                throw new AudioException(AudioExceptionType.Unhandled, "Unexpected exception during start audio recording", ex);
            }
        }

        private void AudioRecorderInfoLisener_OnMaxDurationReached(object sender, EventArgs e)
            => this.OnMaxDurationReached?.Invoke(this, EventArgs.Empty);

        public void Stop()
        {
            if (!this.IsRecording()) return;

            this.recorder.Stop();

            this.duration.Stop();
            this.ReleaseAudioRecorder();
        }

        public bool IsRecording() => this.recorder != null;

        public Stream GetLastRecord()
            => this.fileSystemAccessor.IsFileExists(this.pathToAudioFile)
                ? this.fileSystemAccessor.ReadFile(this.pathToAudioFile)
                : null;

        public TimeSpan GetLastRecordDuration() => this.duration.Elapsed;

        public TimeSpan GetAudioRecordDuration()
        {
            if(!this.fileSystemAccessor.IsFileExists(this.pathToAudioFile))
            {
                return TimeSpan.Zero;
            }

            var metaRetriever = new MediaMetadataRetriever();
            metaRetriever.SetDataSource(pathToAudioFile);
            var durationMs = metaRetriever.ExtractMetadata(MetadataKey.Duration);

            return long.TryParse(durationMs, out long duration) 
                ? TimeSpan.FromMilliseconds(duration) 
                : TimeSpan.Zero;
        }

        public string GetMimeType() => MimeTypeMap.Singleton.GetMimeTypeFromExtension(AudioFileExtension);
        public string GetAudioType() => AudioFileExtension;

        public double GetNoiseLevel()
        {
            if (!this.IsRecording()) return 0;
            return MaxReportableDb + 20 * Math.Log10(this.GetMaxAmplitude() / MaxReportableAmp);
        }

        public NoiseType GetNoiseType(double noiseLevel)
        {
            if(noiseLevel < 45)
                return NoiseType.Low;
            if(noiseLevel > 80)
                return NoiseType.High;
            return NoiseType.Normal;
        }

        private int GetMaxAmplitude()
        {
            var maxAmplitude = 0;
            try
            {
                maxAmplitude = this.recorder.MaxAmplitude;
            }
            catch (RuntimeException)
            {
                /*still don't understand when this exception can be*/
                /* developer.android.com */
                /* int getMaxAmplitude () - Returns the maximum absolute amplitude that was sampled since the last call to this method. Call this only after the setAudioSource(). */
                /* Returns the maximum absolute amplitude measured since the last call, or 0 when called for the first time */
                /* Throws IllegalStateException if it is called before the audio source has been set. */

                /* android.googlesource.com */
                /* 
                static int android_media_MediaRecorder_native_getMaxAmplitude(JNIEnv *env, jobject thiz)
                {
                   ALOGV("getMaxAmplitude");
                   sp<MediaRecorder> mr = getMediaRecorder(env, thiz);
                   int result = 0;
                   process_media_recorder_call(env, mr->getMaxAmplitude(&result), "java/lang/RuntimeException", "getMaxAmplitude failed.");
                   return result;
                }
                */
            }

            return maxAmplitude;
        }

        private void ReleaseAudioRecorder()
        {
            if (this.audioRecorderInfoLisener != null)
            {
                this.audioRecorderInfoLisener.OnMaxDurationReached -= this.AudioRecorderInfoLisener_OnMaxDurationReached;
                this.audioRecorderInfoLisener.Dispose();
                this.audioRecorderInfoLisener = null;
            }

            this.recorder?.Reset();
            this.recorder?.Release();
            this.recorder = null;
        }

        public void Dispose() => this.ReleaseAudioRecorder();
    }
}