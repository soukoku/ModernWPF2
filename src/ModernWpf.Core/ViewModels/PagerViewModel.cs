﻿using System;
using System.Windows.Input;

namespace ModernWpf.ViewModels
{
    /// <summary>
    /// View model for the typical paging logic when given paging size, current page, and total item count.
    /// </summary>
    public class PagerViewModel : ObservableBase
    {
        const int DEFAULT_PG_SZ = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="PagerViewModel"/> class.
        /// </summary>
        public PagerViewModel()
            : this(null, DEFAULT_PG_SZ)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagerViewModel" /> class.
        /// </summary>
        /// <param name="pageChangedCallback">The page changed callback.</param>
        public PagerViewModel(Action<PagerViewModel, int> pageChangedCallback) : this(pageChangedCallback, DEFAULT_PG_SZ)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagerViewModel" /> class.
        /// </summary>
        /// <param name="pageChangedCallback">The page changed callback.</param>
        /// <param name="pageSize">Initial size of the page.</param>
        public PagerViewModel(Action<PagerViewModel, int> pageChangedCallback, int pageSize)
        {
            PageChangedCallback = pageChangedCallback;
            _currentPage = 1;
            _totalPages = 1;
            _pageSize = pageSize > 0 ? pageSize : DEFAULT_PG_SZ;
            LoadProgress = new ProgressViewModel();
        }

        void TryGoToPage(int page)
        {
            if (PageChangedCallback != null)
            {
                PageChangedCallback(this, page);
            }
        }

        /// <summary>
        /// Gets the load progress. Paging commands will be disabled while this is busy.
        /// </summary>
        /// <value>
        /// The load progress.
        /// </value>
        public ProgressViewModel LoadProgress { get; private set; }

        /// <summary>
        /// Gets or sets the page changed callback.
        /// </summary>
        /// <value>
        /// The page changed callback.
        /// </value>
        public Action<PagerViewModel, int> PageChangedCallback { get; set; }

        /// <summary>
        /// To be called by consumers when paged data result changes.
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="totalCount"></param>
        public void UpdateStat(int currentPage, int totalCount)
        {
            if (totalCount < 0) { totalCount = 0; }
            var newTotalPgs = ((totalCount - 1) / PageSize) + 1;

            _currentPage = currentPage;
            _totalPages = newTotalPgs;
            TotalItemCount = totalCount;
            RaisePropertyChanged(() => this.CurrentPage);
            RaisePropertyChanged(() => this.TotalPages);
            RaisePropertyChanged(() => this.CanGoPrevPage);
            RaisePropertyChanged(() => this.CanGoNextPage);
            RaisePropertyChanged(() => this.HasMoreThanOnePage);

            CommandManager.InvalidateRequerySuggested();
        }

        private int _currentPage;
        /// <summary>
        /// Gets or sets the current page.
        /// </summary>
        /// <value>
        /// The current page.
        /// </value>
        public int CurrentPage
        {
            get { return _currentPage > _totalPages ? _totalPages : _currentPage; }
            set
            {
                if (value > 0)
                    TryGoToPage(value);
            }
        }

        private int _pageSize;
        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>
        /// The size of the page.
        /// </value>
        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                if (_pageSize > 0)
                {
                    _pageSize = value;
                    RaisePropertyChanged(() => this.PageSize);
                    TryGoToPage(CurrentPage);
                }
            }
        }

        private int _totalItemCount;

        /// <summary>
        /// Gets the total item count across all pages.
        /// </summary>
        /// <value>
        /// The total item count.
        /// </value>
        public int TotalItemCount
        {
            get { return _totalItemCount; }
            private set
            {
                _totalItemCount = value;
                RaisePropertyChanged(() => TotalItemCount);
            }
        }

        private int _totalPages;
        /// <summary>
        /// Gets the total pages.
        /// </summary>
        /// <value>
        /// The total pages.
        /// </value>
        public int TotalPages
        {
            get { return _totalPages; }
            private set
            {
                _totalPages = value;
                RaisePropertyChanged(() => this.TotalPages);
            }
        }



        private RelayCommand _reloadCommand;
        /// <summary>
        /// Gets the reload command that reloads the current page.
        /// </summary>
        /// <value>
        /// The reload command.
        /// </value>
        public ICommand ReloadCommand
        {
            get
            {
                return _reloadCommand ?? (
                    _reloadCommand = new RelayCommand(() =>
                    {
                        TryGoToPage(CurrentPage);
                    }, () => !LoadProgress.IsBusy)
                );
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has more than one page.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has more than one page; otherwise, <c>false</c>.
        /// </value>
        public bool HasMoreThanOnePage { get { return TotalPages > 0; } }

        /// <summary>
        /// Gets a value indicating whether previous page is allowed.
        /// </summary>
        /// <value>
        /// </value>
        public bool CanGoPrevPage { get { return !LoadProgress.IsBusy && CurrentPage > 1 && TotalPages > 1; } }

        /// <summary>
        /// Gets a value indicating whether next page is allowed.
        /// </summary>
        /// <value>
        /// </value>
        public bool CanGoNextPage { get { return !LoadProgress.IsBusy && CurrentPage < TotalPages; } }

        private RelayCommand _firstPageCommand;
        /// <summary>
        /// Gets the command to go to the first page
        /// </summary>
        /// <value>
        /// The first page command.
        /// </value>
        public ICommand FirstPageCommand
        {
            get
            {
                return _firstPageCommand ?? (
                    _firstPageCommand = new RelayCommand(() =>
                    {
                        TryGoToPage(1);
                    }, () => CanGoPrevPage)
                );
            }
        }


        private RelayCommand _prevPageCommand;
        /// <summary>
        /// Gets the command to go to the previous page
        /// </summary>
        /// <value>
        /// The previous page command.
        /// </value>
        public ICommand PrevPageCommand
        {
            get
            {
                return _prevPageCommand ?? (
                    _prevPageCommand = new RelayCommand(() =>
                    {
                        TryGoToPage(CurrentPage - 1);
                    }, () => CanGoPrevPage)
                );
            }
        }

        private RelayCommand _nextPageCommand;
        /// <summary>
        /// Gets the command to go to the next page
        /// </summary>
        /// <value>
        /// The next page command.
        /// </value>
        public ICommand NextPageCommand
        {
            get
            {
                return _nextPageCommand ?? (
                    _nextPageCommand = new RelayCommand(() =>
                    {
                        TryGoToPage(CurrentPage + 1);
                    }, () => CanGoNextPage)
                );
            }
        }


        private RelayCommand _lastPageCommand;
        /// <summary>
        /// Gets the command to go to the last page
        /// </summary>
        /// <value>
        /// The last page command.
        /// </value>
        public ICommand LastPageCommand
        {
            get
            {
                return _lastPageCommand ?? (
                    _lastPageCommand = new RelayCommand(() =>
                    {
                        TryGoToPage(TotalPages);
                    }, () => CanGoNextPage)
                );
            }
        }

    }
}
