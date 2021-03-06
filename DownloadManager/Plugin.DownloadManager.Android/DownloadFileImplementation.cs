﻿using System.Collections.Generic;
using System.ComponentModel;
using Android.App;
using Android.Database;
using Android.Net;
using Plugin.DownloadManager.Abstractions;

namespace Plugin.DownloadManager
{
    public class DownloadFileImplementation : IDownloadFile
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public long Id;

        public string Url { get; set; }

        public IDictionary<string, string> Headers { get; private set; }

        DownloadFileStatus _status;

        public DownloadFileStatus Status {
            get {
                return _status;
            }
            set {
                if (!Equals(_status, value)) {
                    _status = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
                }
            }
        }

        string _statusDetails;

        public string StatusDetails {
            get {
                return _statusDetails;
            }
            set {
                if (!Equals(_statusDetails, value)) {
                    _statusDetails = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatusDetails"));
                }
            }
        }

        float _totalBytesExpected = 0.0f;

        public float TotalBytesExpected {
            get {
                return _totalBytesExpected;
            }
            set {
                if (!Equals(_totalBytesExpected, value)) {
                    _totalBytesExpected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalBytesExpected"));
                }
            }
        }

        float _totalBytesWritten = 0.0f;

        public float TotalBytesWritten {
            get {
                return _totalBytesWritten;
            }
            set {
                if (!Equals(_totalBytesWritten, value)) {
                    _totalBytesWritten = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalBytesWritten"));
                }
            }
        }

        /**
         * Initializing a new object to add it to the download-queue
         */
        public DownloadFileImplementation(string url, IDictionary<string, string> headers)
        {
            Url = url;
            Headers = headers;

            Status = DownloadFileStatus.INITIALIZED;
        }

        /**
         * Reinitializing an object after the app restarted
         */
        public DownloadFileImplementation(ICursor cursor)
        {
            Id = cursor.GetLong(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnId));
            Url = cursor.GetString(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnUri));

            switch ((DownloadStatus)cursor.GetInt(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnStatus))) {
                case DownloadStatus.Failed:
                    Status = DownloadFileStatus.FAILED;
                    break;
                    
                case DownloadStatus.Paused:
                    Status = DownloadFileStatus.PAUSED;
                    break;

                case DownloadStatus.Pending:
                    Status = DownloadFileStatus.PENDING;
                    break;

                case DownloadStatus.Running:
                    Status = DownloadFileStatus.RUNNING;
                    break;

                case DownloadStatus.Successful:
                    Status = DownloadFileStatus.COMPLETED;
                    break;
            }
        }

        public void StartDownload(Android.App.DownloadManager downloadManager, string destinationPathName, bool allowedOverMetered)
        {
            using (var downloadUrl = Uri.Parse(Url))
            using (var request = new Android.App.DownloadManager.Request(downloadUrl)) {
                if (Headers != null) {
                    foreach (var header in Headers) {
                        request.AddRequestHeader(header.Key, header.Value);
                    }
                }

                if (destinationPathName != null) {
                    request.SetDestinationUri(Uri.FromFile(new Java.IO.File(destinationPathName)));
                }

                request.SetAllowedOverMetered(allowedOverMetered);

                Id = downloadManager.Enqueue(request);
            }
        }
    }
}
