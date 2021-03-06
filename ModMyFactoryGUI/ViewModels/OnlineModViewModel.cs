//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Media.Imaging;
using ModMyFactory.WebApi.Mods;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class OnlineModViewModel : ReactiveObject, IDisposable
    {
        private static readonly ModReleaseInfo[] _emptyReleases = new ModReleaseInfo[0];

        private ApiModInfo _info;
        private bool _isExtended;

        private bool isDisposed = false;

        public ApiModInfo Info
        {
            get => _info;
            private set
            {
                this.RaiseAndSetIfChanged(ref _info, value, nameof(Info));

                this.RaisePropertyChanged(nameof(DisplayName));
                this.RaisePropertyChanged(nameof(Author));
                this.RaisePropertyChanged(nameof(DownloadCount));
                this.RaisePropertyChanged(nameof(Summary));
                this.RaisePropertyChanged(nameof(Description));
                this.RaisePropertyChanged(nameof(HasChangelog));
                this.RaisePropertyChanged(nameof(Changelog));
                this.RaisePropertyChanged(nameof(HasFaq));
                this.RaisePropertyChanged(nameof(Faq));
                this.RaisePropertyChanged(nameof(Releases));
            }
        }

        public bool HasThumbnail { get; private set; }
        public IBitmap Thumbnail { get; private set; }

        public bool HasHomepage { get; private set; }
        public Uri Homepage { get; private set; }

        public bool HasGitHub { get; private set; }
        public Uri GitHubUrl { get; private set; }

        public bool HasLicense { get; private set; }
        public string LicenseName { get; private set; }
        public bool HasLicenseUrl { get; private set; }
        public Uri LicenseUrl { get; private set; }

        public bool HasChangelog => !string.IsNullOrWhiteSpace(Changelog);
        public bool HasFaq => !string.IsNullOrWhiteSpace(Faq);
        public string Changelog => Info.Changelog;
        public string Faq => Info.Faq;

        public string DisplayName => Info.DisplayName;

        public string Author => Info.Author;

        public int DownloadCount => Info.DownloadCount;

        public string Summary => Info.Summary;

        public string Description => Info.Description;

        public IReadOnlyCollection<ModReleaseInfo> Releases => _isExtended ? Info.Releases ?? _emptyReleases : _emptyReleases;

        public OnlineModViewModel(ApiModInfo info)
            => _info = info;

        ~OnlineModViewModel()
        {
            Dispose(false);
        }

        private void LoadHomepage()
        {
            HasHomepage = Uri.TryCreate(Info.Homepage, UriKind.Absolute, out var homepage);
            this.RaisePropertyChanged(nameof(HasHomepage));
            if (HasHomepage)
            {
                Homepage = homepage;
                this.RaisePropertyChanged(nameof(Homepage));
            }
        }

        private void LoadGitHub()
        {
            HasGitHub = Uri.TryCreate(Info.GitHubUrl, UriKind.Absolute, out var gitHubUrl);
            this.RaisePropertyChanged(nameof(HasGitHub));
            if (HasGitHub)
            {
                GitHubUrl = gitHubUrl;
                this.RaisePropertyChanged(nameof(GitHubUrl));
            }
        }

        private void LoadLicense()
        {
            LicenseName = Info.License.Name;
            this.RaisePropertyChanged(nameof(LicenseName));
            HasLicense = !string.IsNullOrWhiteSpace(LicenseName);
            this.RaisePropertyChanged(nameof(HasLicense));
            if (HasLicense)
            {
                HasLicenseUrl = Uri.TryCreate(Info.License.Url, UriKind.Absolute, out var licenseUrl);
                this.RaisePropertyChanged(nameof(HasLicenseUrl));
                if (HasLicenseUrl)
                {
                    LicenseUrl = licenseUrl;
                    this.RaisePropertyChanged(nameof(LicenseUrl));
                }
            }
        }

        private async Task LoadThumbnail()
        {
            HasThumbnail = !string.IsNullOrWhiteSpace(Info.ThumbnailUrl);
            this.RaisePropertyChanged(nameof(HasThumbnail));
            if (HasThumbnail)
            {
                if (Uri.TryCreate(Info.ThumbnailUrl, UriKind.Absolute, out Uri url))
                {
                    using var wc = new WebClient();
                    try
                    {
                        byte[] data = await wc.DownloadDataTaskAsync(url);
                        using var stream = new MemoryStream(data);
                        Thumbnail = new Bitmap(stream);
                        this.RaisePropertyChanged(nameof(Thumbnail));
                    }
                    catch (WebException)
                    {
                        // This is only loading the thumbnail, so it's ok to just agressively catch all web exceptions.
                    }
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                Thumbnail?.Dispose();
                isDisposed = true;
            }
        }

        public async Task LoadExtendedInfoAsync()
        {
            if (!_isExtended)
            {
                _isExtended = true;
                Info = await ModApi.RequestModInfoAsync(Info.Name);

                LoadHomepage();
                LoadGitHub();
                LoadLicense();
                await LoadThumbnail();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
