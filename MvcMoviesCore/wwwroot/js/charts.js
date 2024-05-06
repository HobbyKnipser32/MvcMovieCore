var uri;
var uri_missingValue;
var title;
var missingText;
var today;
var totalOn;

function addHiddenField(frm, key, value) {
    var elem = document.createElement('input');
    elem.type = 'hidden';
    elem.name = key;
    elem.id = key;
    elem.value = value;
    frm.appendChild(elem);
}

function removeHiddenField(frm, key) {
    $('#' + key).remove();
}

$(document).ready(function () {
    var chartfx;

    $('#showChart').click(function () {
        $('#barchartnav').hide();
        var table = $('#vehicleTable').DataTable();
        $("#chart").css({ display: "block" });
        $("#hideChart").css({ display: "block" });
        $("#GeneralStatusModalBody").css({ display: "none" });
        $("#chart").empty();

        var form = document.getElementById("filterForm");
        form.setAttribute("method", "post");
        form.setAttribute("action", uri);

        var vehicleIds = [];
        table.rows({ search: 'applied' }).every(function () {
            vehicleIds.push(this.id());
        });

        addHiddenField(form, 'vehicleIds', vehicleIds.join());
        var startDate = $("#dtReportDateFrom").data("DateTimePicker").date();
        if (startDate)
            addHiddenField(form, 'startDate', startDate.toISOString());

        var endDate = $("#dtReportDateTo").data("DateTimePicker").date();
        if (endDate)
            addHiddenField(form, 'endDate', endDate.toISOString());

        var reportingDate = $("#dtReportDate").data("DateTimePicker").date();
        if (reportingDate)
            addHiddenField(form, 'reportingDate', reportingDate.toISOString());

        addHiddenField(form, 'timeUnit', $("input[name='timeUnit']:checked").val());
        addHiddenField(form, 'groupBy', $("input[name='groupBy']:checked").val());
        addHiddenField(form, 'showTotal', $("#showTotals").is(":checked"));
        addHiddenField(form, 'departmentOptions', $("#departmentOptions").val());
        addHiddenField(form, 'department', $("#departments").val());
        $("#chartLoader").show();
        $.ajax({
            url: uri,
            type: 'POST',
            data: $('#filterForm').serialize(),
            success: function (response) {
                $("#chartLoader").hide();

                removeHiddenField(form, 'vehicleIds');
                removeHiddenField(form, 'startDate');
                removeHiddenField(form, 'endDate');
                removeHiddenField(form, 'reportingDate');
                removeHiddenField(form, 'timeUnit');
                removeHiddenField(form, 'groupBy');
                removeHiddenField(form, 'showTotal');
                removeHiddenField(form, 'departmentOptions');
                removeHiddenField(form, 'department');

                if (response.summary.length !== 0 && $("input[name='groupBy']:checked").val() !== 'target') {
                    if (chartfx) {
                        chartfx.destroy();
                        chartfx = undefined;
                        $("#chart").empty();
                    }

                    var markup = '<div style=\"max-height: 600px; overflow: auto;\">';
                    markup += '<p><h3>' + missingText + '</h3></p>';
                    response.summary.forEach(function (element) {
                        markup += '<p><a target=\"_blank\" rel=\"noopener noreferrer\" href=\"' + uri_missingValue + '/' + element.id + '\">' + element.pNr + '</a></p>';
                    });
                    $('#chart').append(markup + '</div>');
                } else {
                    var isFinasTargetReport = $("#rdpproject").is(":checked")
                        || $("#finasdepartments").is(":checked")
                        || $("#departmentproject").is(":checked");
                    var options = {};
                    if (isFinasTargetReport) {
                        var selectedReport = null;
                        if ($("#rdpproject").is(":checked"))
                            selectedReport = $("#rdpproject");
                        else if ($("#finasdepartments").is(":checked"))
                            selectedReport = $("#finasdepartments");
                        else if ($("#departmentproject").is(":checked"))
                            selectedReport = $("#departmentproject");

                        var columnChart = new FinasTargetChart(response, selectedReport)
                        chartfx = columnChart.refresh();
                    }
                    else {
                        var options = getChartOptions(response);
                        if (options) {
                            if (!chartfx)
                                chartfx = new ApexCharts(document.querySelector("#chart"), options);
                            else
                                chartfx.updateSeries(options.series);

                            chartfx.render();
                        }
                    }


                }
            }
        });
    });

    $("#showChart").attr("disabled", true);

    $("#dtReportDateFrom,#dtReportDateTo").bind("blur change paste keyup", function () {
        var startDate = $("#dtReportDateFrom").data("DateTimePicker").date();
        var endDate = $("#dtReportDateTo").data("DateTimePicker").date();
        $("#showChart").attr("disabled", startDate === null || endDate === null || startDate >= endDate);
    });

    $('#hideChart').click(function () {
        if (chartfx) {
            chartfx.destroy();
            chartfx = undefined;
            $("#chart").empty();
        }
        $("#chart").css({ display: "none" });
        $("#hideChart").css({ display: "none" });
        $("#GeneralStatusModalBody").css({ display: "block" });
        $('#barchartnav').hide();
        resetDepartmentOptions();
    });

    $('#ReportingPopup').on('show.bs.modal',
        function () {
            var config = $(this).data('config');
            uri = config.uri;
            uri_missingValue = config.uri_missingValue;
            title = config.title;
            missingText = config.missingText;
            today = config.today;
            totalOn = config.totalOn;

            if (chartfx) {
                chartfx.destroy();
                chartfx = undefined;
            }
            $("#chart").css({ display: "none" });
            $("#hideChart").css({ display: "none" });
            $("#GeneralStatusModalBody").css({ display: "block" });
            resetDepartmentOptions();
        });

    resetDepartmentOptions();
    resetDepartments();
    
    var getReportingDateTotal = function (series) {
        return series.find(s => s.name == 'ReportingDateTotal').data.reduce((a, b) => a + b, 0);
    }

    var getChartOptions = function (response) {
        var options = response.data;
        options.dataLabels = {
            enabled: $("#showColumnValues").is(":checked"),
            position: 'top',
            floating: true,
            offsetY: -4,
            background: {
                enabled: false
            }
        };

        options.stroke = {
            width: 2
        };

        options.legend = {
            show: $("#showLegend").is(":checked"),
            position: 'right',
            horizontalAlign: 'center',
            offsetY: 50
        };

        options.annotations = {};

        if ($("#showMax").is(":checked")) {
            options.annotations.yaxis = [
                {
                    y: response.max,
                    borderColor: '#222222',
                    label: {
                        borderColor: '#222222',
                        style: {
                            color: '#fff',
                            background: '#222222'
                        },
                        text: 'Max: ' + response.max
                    }
                }
            ];
        }

        if ($("#showCurrentDate").is(":checked")) {
            options.annotations.xaxis = [
                {
                    x: new Date().getTime(),
                    strokeDashArray: 0,
                    borderColor: '#00E396',
                    label: {
                        borderColor: '#00E396',
                        style: {
                            color: '#fff',
                            background: '#00E396'
                        },
                        orientation: "horizontal",
                        text: today
                    }
                },
                {
                    x: new Date(moment($('#dtReportDate').val(), 'DD.MM.YYYY') - moment('01.01.1970', 'DD.MM.YYYY')).getTime(),
                    strokeDashArray: 0,
                    borderColor: '#1a00e4',
                    label: {
                        borderColor: '#1a00e4',
                        style: {
                            color: '#fff',
                            background: '#1a00e4'
                        },
                        orientation: "horizontal",
                        text: totalOn + ' ' + $('#dtReportDate').val() + ' :' + response.reportingDateTotal
                    }
                }
            ];
        }

        options.title = {
            text: title,
            align: 'left'
        };

        options.xaxis.type = 'datetime';
        options.xaxis.labels = {
            show: true,
            hideOverlappingLabels: false,
            formatter: function (value, timestamp, index) {
                var dt = moment(new Date(timestamp));
                var dtype = $("input[name='timeUnit']:checked").val();

                if (dtype === 'week')
                    return dt.week() + dt.format("/YYYY");

                if (dtype === 'month')
                    return dt.format("MMM YY");

                return dt.format("MMM YYYY");
            }
        };
        options.colors = ['#00677F', '#77B6EA', '#545454', '#999999', '#1A5687'];

        options.chart = {
            type: 'line',
            toolbar: {
                show: true,
                tools: {
                    download: true,
                    selection: true,
                    zoom: true,
                    zoomin: true,
                    zoomout: true,
                    pan: true,
                    reset: true | '<img src="/static/icons/reset.png" width="20">',
                    customIcons: []
                },
                autoSelected: 'zoom'
            }
        };

        return options;
    }

    $("[name='groupBy']").click(function () {
        resetDepartmentOptions();
        resetDepartments();
        loadDepartments();
    });
    function resetDepartmentOptions() {
        var isOrganizationSelected = $('#organization').is(':checked');
        if (isOrganizationSelected)
            $('#departmentOptionsContainer').show();
        else
            $('#departmentOptionsContainer').hide();
    }
    function resetDepartments() {
        var isTargetByFinasDepartmentsSelected = $('#departmentproject').is(':checked');
        if (isTargetByFinasDepartmentsSelected)
            $('#departmentsContainer').show();
        else
            $('#departmentsContainer').hide();
    }
    function loadDepartments() {
        var url = $('#departments').data('url');
        $.get(url, function (items) {
            $('#departments').empty();
                $.each(items, function (i, item) {
                    $('#departments').append($('<option>', {
                        value: item,
                        text: item
                    }));
                });
            });
    }
    
});

class FinasTargetChart {
    static chartData = {};
    static chartInstance = null;
    static _selectedReport = null;
    constructor(chartData, selectedReport) {
        FinasTargetChart.chartData = chartData;
        FinasTargetChart._selectedReport = selectedReport;
        this._initialize();
        this._resetSeriesBoundary();
    }

    //Fields
    static get _step() {
        return 15;
    }
    get _currentPageData() {
        var startIndex = $('#columnChartPageState').data('start-index');
        var endIndex = $('#columnChartPageState').data('end-index');
        return FinasTargetChart.chartData.items.slice(startIndex, endIndex);
    }

    get _options() {
        $('#barchartnav').show();
        var options = FinasTargetChart.chartData.data;
        var items = FinasTargetChart.chartData.items;
        var boundary = FinasTargetChart.prototype._getPageBoundary();
        if (boundary.startIndex > items.length)
            return null;

        options.dataLabels = {
            enabled: $("#showColumnValues").is(":checked"),
            position: 'top',
            floating: true,
            background: {
                enabled: false
            },
            offsetY: -20,
            style: {
                fontSize: '12px',
                colors: ["#304758"]
            }
        };
        options.plotOptions = {
            bar: {
                columnWidth: '80%',
                dataLabels: {
                    position: 'top'
                },
            }
        };
        options.stroke = {
            width: 2
        };

        options.legend = {
            show: $("#showLegend").is(":checked"),
            position: 'right',
            horizontalAlign: 'center',
            offsetY: 50
        };

        options.annotations = {};

        if ($("#showMax").is(":checked")) {
            options.annotations.yaxis = [
                {
                    y: FinasTargetChart.chartData.max,
                    borderColor: '#222222',
                    label: {
                        borderColor: '#222222',
                        style: {
                            color: '#fff',
                            background: '#222222'
                        },
                        text: 'Max: ' + FinasTargetChart.chartData.max
                    }
                }
            ];
        }

        if ($("#showCurrentDate").is(":checked")) {
            options.annotations.xaxis = [
                {
                    x: new Date().getTime(),
                    strokeDashArray: 0,
                    borderColor: '#00E396',
                    label: {
                        borderColor: '#00E396',
                        style: {
                            color: '#fff',
                            background: '#00E396'
                        },
                        orientation: "horizontal",
                        text: today
                    }
                },
                {
                    x: new Date(moment($('#dtReportDate').val(), 'DD.MM.YYYY') - moment('01.01.1970', 'DD.MM.YYYY')).getTime(),
                    strokeDashArray: 0,
                    borderColor: '#1a00e4',
                    label: {
                        borderColor: '#1a00e4',
                        style: {
                            color: '#fff',
                            background: '#1a00e4'
                        },
                        orientation: "horizontal",
                        text: totalOn + ' ' + $('#dtReportDate').val() + ' :' + FinasTargetChart.chartData.reportingDateTotal
                    }
                }
            ];
        }

        var title = $(FinasTargetChart._selectedReport).data('title');
        if ($(FinasTargetChart._selectedReport).attr('id') == 'departmentproject')
            title += ' - ' + $('#departments').val();

        options.title = {
            text: title,
            align: 'left'
        };

        var totalVehiclesOnReportingDay = items.flatMap(i => i.item.reportingDay.count).reduce((a, b) => a + b, 0);
        var reportingDate = $("#dtReportDate").data("DateTimePicker").date();
        var subtitleText = $(FinasTargetChart._selectedReport).data('subtitle')
            + " (" + moment(reportingDate).format("DD.MM.YYYY") + "): "
            + totalVehiclesOnReportingDay + " " + $(FinasTargetChart._selectedReport).data('subtitle-vehicles');
        options.subtitle = {
            text: subtitleText,
            align: 'left'
        };

        options.xaxis.labels = {
            show: true,
            hideOverlappingLabels: false,
            rotate: -45,
            rotateAlways: true,
            minHeight: 100,
            maxHeight: 200,
        };
        options.xaxis.tickPlacement = 'on';
        options.colors = ['#808080', '#ff0000', '#050404', '#88d188'];
        options.yaxis.max = FinasTargetChart.chartData.max;
        options.chart = {
            type: 'bar',
            toolbar: {
                show: true,
                tools: {
                    download: true,
                    selection: true,
                    zoom: true,
                    zoomin: true,
                    zoomout: true,
                    pan: true,
                    reset: true | '<img src="/static/icons/reset.png" width="20">',
                    customIcons: []
                },
                autoSelected: 'zoom'
            },
            events: {
                dataPointSelection: (event, chartContext, config) => {
                    if ($.fn.dataTable.isDataTable('#finasVehiclesTable'))
                        $('#finasVehiclesTable').DataTable().destroy();

                    var project = FinasTargetChart.prototype._currentPageData[config.dataPointIndex];
                    var seriesItem = {};
                    if (config.seriesIndex == 0)
                        seriesItem = project.item.reportingDay;
                    else if (config.seriesIndex == 1)
                        seriesItem = project.item.forecast;
                    else if (config.seriesIndex == 2)
                        seriesItem = project.item.excluded;
                    else if (config.seriesIndex == 3)
                        seriesItem = project.item.target;

                    var associatedVehicles = [];
                    if (seriesItem && seriesItem.vehicles)
                        associatedVehicles = seriesItem.vehicles;
                    var cultureUrl = $('#cultureUrl').data('url');
                    $('#finasVehiclesTable').DataTable({
                        autoWidth: false,
                        language: {
                            url: cultureUrl
                        },
                        data: associatedVehicles,
                        columns: [
                            {
                                data: 'powertrainProjectName',
                                width: '15%'
                            },
                            {
                                data: 'vehicleCode',
                                width: '15%'
                            },
                            {
                                data: 'transferDate',
                                render: function (data, type, full, meta) {
                                    if (data != null) {
                                        if (type === 'display') {
                                            return moment(data).calendar();
                                        }
                                        else {
                                            return data;
                                        }
                                    }
                                    return '';
                                },
                                width: '12%'
                            },
                            {
                                data: 'deliveryDate',
                                render: function (data, type, full, meta) {
                                    data = getUsageEndsOn();
                                    if (data != null) {
                                        if (type === 'display') {
                                            return moment(data).calendar();
                                        }
                                        else {
                                            return data;
                                        }
                                    }
                                    return '';
                                    function getUsageEndsOn() {
                                        var usageEndsOn = full["usageEndsOn"];
                                        var plannedUsageEndDate = full["plannedUsageEndDate"];
                                        if (plannedUsageEndDate && moment(plannedUsageEndDate).year() > 2000)
                                            return plannedUsageEndDate;
                                        else
                                            return usageEndsOn;
                                    }
                                },
                                width: '13%'
                            },
                            {
                                data: 'budgetYear',
                                render: function (data, type, full, meta) {
                                    if (data) {
                                        return data;
                                    }
                                    return '-';
                                },
                                width: '10%'
                            },
                            {
                                data: 'responsiblePersonAndDepartment',
                                width: '25%'
                            },
                            {
                                data: 'vehicleStatus',
                                width: '10%'
                            }
                        ],
                        order: [[0, 'desc']],
                        searching: true,
                        paging: true,
                        pageLength: 5,
                        fixedHeader: {
                            header: false,
                            footer: true
                        },
                        processing: true,
                        "bLengthChange": false,
                        dom: 'Blfrtip',
                        buttons: [
                            { extend: 'excel', className: 'btn-rose' },
                        ]
                    });

                    var seriesName = config.w.config.series[config.seriesIndex].name;
                    var seriesColor = config.w.config.colors[config.seriesIndex];
                    $('#FinasVehiclesModalTitle').text(project.key + ' - ' + seriesName);
                    $('#FinasVehiclesModalTitleIcon').css('color', seriesColor);
                    $('#FinasVehiclesModal').modal('show');
                }
            }
        };

        options.grid = {
            padding: {
                top: 0,
                right: 25,
                bottom: 0,
                left: 25
            }
        };

        var seriesData = FinasTargetChart.prototype._sliceColumnChartSeries(boundary.startIndex, boundary.endIndex);
        options.series = seriesData;

        var categoriesData = FinasTargetChart.prototype._sliceColumnChartCategories(boundary.startIndex, boundary.endIndex);
        options.xaxis.categories = categoriesData;

        return options;
    }

    //Methods
    _initialize() {
        $('#nextPage').unbind('click').click(function () {
            FinasTargetChart.prototype._nextPage();
            FinasTargetChart.prototype.refresh();
        });
        $('#previousPage').unbind('click').click(function () {
            FinasTargetChart.prototype._previousPage();
            FinasTargetChart.prototype.refresh();
        });
    }

    refresh() {
        var options = FinasTargetChart.prototype._options;
        var chartfx = FinasTargetChart.chartInstance;
        if (options) {
            if (chartfx) {
                chartfx.destroy();
                chartfx = undefined;
                $("#chart").empty();
            }

            chartfx = new ApexCharts(document.querySelector("#chart"), options);
            chartfx.render();
            FinasTargetChart.chartInstance = chartfx;
            FinasTargetChart.prototype._refreshPageNavigation();
        }
        return chartfx;
    }

    _getPageBoundary() {
        return {
            startIndex: $('#columnChartPageState').data('start-index'),
            endIndex: $('#columnChartPageState').data('end-index')
        };
    }
    _nextPage() {
        var maxCategoryIndex = FinasTargetChart.chartData.items.length;
        var currentStartIndex = $('#columnChartPageState').data('start-index');
        var currentEndIndex = $('#columnChartPageState').data('end-index');
        
        var start = currentStartIndex + FinasTargetChart._step;
        var end = Math.min(currentEndIndex + FinasTargetChart._step, maxCategoryIndex);
        if (start > maxCategoryIndex)
            return null;

        $('#columnChartPageState').data('start-index', start);
        $('#columnChartPageState').data('end-index', end);
        return FinasTargetChart.prototype._getPageBoundary();
    }
    _previousPage() {
        var minCategoryIndex = 0;
        var maxCategoryIndex = FinasTargetChart.chartData.items.length;
        var currentStartIndex = $('#columnChartPageState').data('start-index');
        var currentEndIndex = $('#columnChartPageState').data('end-index');
        var start = Math.max(currentStartIndex - FinasTargetChart._step, minCategoryIndex);
        var end = Math.min(start + FinasTargetChart._step, maxCategoryIndex);
        if (start >= end)
            return null;

        $('#columnChartPageState').data('start-index', start);
        $('#columnChartPageState').data('end-index', end);
        return FinasTargetChart.prototype._getPageBoundary();
    }
    _resetSeriesBoundary() {
        $('#columnChartPageState').data('start-index', 0);
        $('#columnChartPageState').data('end-index', 15);
    }
    _refreshPageNavigation() {
        var boundary = FinasTargetChart.prototype._getPageBoundary();
        if (boundary.endIndex >= FinasTargetChart.chartData.items.length) {
            $('#nextPage').addClass('disabled');
        }
        else {
            $('#nextPage').removeClass('disabled');
        }
        if (boundary.startIndex == 0) {
            $('#previousPage').addClass('disabled');
        }
        else {
            $('#previousPage').removeClass('disabled');
        }

        var pages = Math.ceil(FinasTargetChart.chartData.items.length / FinasTargetChart._step);
        var currentPage = Math.ceil(boundary.endIndex / FinasTargetChart._step);
        $('#columnChartPageState').data('current-page', currentPage);
        $('#pageNo').text(currentPage + '/' + pages);
    }
    _sliceColumnChartSeries(start, end) {
        if (FinasTargetChart.chartData.items.length <= 0)
            return null;

        var reportingDayColumn, targetColumn, forecastColumn, excludedColumn = null;
        if (FinasTargetChart.chartData.items[0].item.reportingDay)
            reportingDayColumn = {
                name: FinasTargetChart.chartData.items[0].item.reportingDay.title,
                data: []
            };
        if (FinasTargetChart.chartData.items[0].item.forecast)
            forecastColumn = {
                name: FinasTargetChart.chartData.items[0].item.forecast.title,
                data: []
            };
        if (FinasTargetChart.chartData.items[0].item.excluded)
            excludedColumn = {
                name: FinasTargetChart.chartData.items[0].item.excluded.title,
                data: []
            };
        if (FinasTargetChart.chartData.items[0].item.target)
            targetColumn = {
                name: FinasTargetChart.chartData.items[0].item.target.title,
                data: []
            };

        var itemsToBeShown = FinasTargetChart.chartData.items.slice(start, end);
        itemsToBeShown.forEach(function (val, i) {
            if (reportingDayColumn)
                reportingDayColumn.data.push(val.item.reportingDay.count);
            if (forecastColumn)
                forecastColumn.data.push(val.item.forecast.count);
            if (excludedColumn)
                excludedColumn.data.push(val.item.excluded.count);
            if (targetColumn)
                targetColumn.data.push(val.item.target.count);
        });
        var result = [];
        if (reportingDayColumn)
            result.push(reportingDayColumn);
        if (forecastColumn)
            result.push(forecastColumn);
        if (excludedColumn)
            result.push(excludedColumn);
        if (targetColumn)
            result.push(targetColumn);
        return result;
    }
    _sliceColumnChartCategories(start, end) {
        return FinasTargetChart.chartData.items
            .filter((val, i) => i >= start && i <= end)
            .map((val, i) => val.key)
    }
}