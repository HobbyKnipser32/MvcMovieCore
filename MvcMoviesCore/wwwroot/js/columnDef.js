var availableColumns;
var uri;
var uri_all;
var callbackUri;
var columnDefs;

function addRow(item) {
    if (item === 'Id')
        return;

    var li = document.createElement('li');
    li.className = 'list-group-item';

    var icon = document.createElement('i');
    icon.className = 'fas fa-grip-vertical handle';
    li.append(icon);

    li.append(addSelect2(item));

    var span = document.createElement('div');
    span.onclick = () => removeColumn(li, item);

    var close = document.createElement('i');
    close.className = 'fas fa-times removeColumn';
    span.append(close);
    li.append(span);

    $("#rows").append(li);
}

function resetRows() {
    var element = $("#rows");
    element.children().remove();
}

function addSelect2(item) {
    var select = document.createElement('select');
    select.className = 'form-control select2-dropdown colunDef';
    select.name = item;
    select.id = item;

    Object.keys(availableColumns).forEach(i => {
        if (i === item || !columnDefs.includes(i)) {
            var options = document.createElement('option');
            options.appendChild(document.createTextNode(availableColumns[i]));
            options.value = i;
            select.appendChild(options);
        }
    });

    select.value = item;

    return select;
}

function removeColumn(li, item) {
    var i = columnDefs.indexOf(item);
    columnDefs.splice(i, 1);

    li.remove();
}

$(document).ready(function () {
    $('#columnDefPopup').on('show.bs.modal',
        function () {
            var config = $(this).data('config');
            uri = config.uri;
            uri_all = config.uri_all;
            callbackUri = config.callback;

            $.get(uri_all,
                function (data) {
                    availableColumns = data;

                    $.get(uri,
                        function(data) {
                            columnDefs = Array.from(Object.keys(data));
                            resetRows();
                            columnDefs.forEach(item => {
                                addRow(item);
                            });
                            InitSelect2Dropdown();
                        });
                });

            Sortable.create(rows, {
                animation: 150,
                handle: '.handle',
                group: 'list-1',
                draggable: '.list-group-item',
                sort: true,
                filter: '.sortable-disabled',
                chosenClass: 'active'
            });
        });

    $('#addNewColumnRow').on('click',
        function () {
            addRow('');
            InitSelect2Dropdown();
        });

    function InitSelect2Dropdown() {
        $('.select2-dropdown').not('#onBehalfUsers,[multiple]').each(function (i, elem) {
            $(elem).select2({
                width: '100%',
                placeholder: $('#_pleaseChoose').val(),
                allowClear: true,
                escapeMarkup: function (markup) {
                    return markup;
                },
                "language": {
                    "noResults": function () {
                        return $('#_NoResultsFound').val();
                    }
                },
                templateResult: function (data, container) {
                    var description = $(data.element).data("description");
                    if (description) {
                        return '<div><strong>' + data.text + '</strong></div>' + '<div><small>' + description + '</small></div>';
                    }
                    else
                        return data.text;
                },
                matcher: matchCustom
            });
        });
    }
    $('#saveColumnDef').on('click',
        function () {
            var elements = Array.from($(".colunDef"));
            var items = elements.map(row => {
                return row.value;
            });

            $.ajax({
                url: uri,
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                data: JSON.stringify(items),
                type: 'post',
                dataType: 'json',
                error: function (response) {
                    console.log("Error: " + response);
                },
                success: function () {
                    $('#columnDefPopup').modal('toggle');
                    window.location.href = callbackUri;
                }
            });
        });
});