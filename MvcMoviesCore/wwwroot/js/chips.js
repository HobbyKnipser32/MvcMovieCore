class filter {
    static createChips() {
        chip.removeAll('chips');

        $.each($('#filterForm :input:not([type=hidden])'), function (index, field) {
            if ($(field).val().length !== 0) {
                var tooltip = chip.getTooltip($(field).attr('id'));
                if ($(field).hasClass('select2-dropdown')) {
                    $(field).select2('data').forEach(function (data) {
                        if(data.text != '')
                            chip.create('chips', field.name, data.id, data.text, tooltip);
                    });
                }
                else if ($(field).is('[type=checkbox]'))
                {
                    if ($(field).prop("checked"))
                        chip.create('chips', field.name, $(field).prop("checked"), $(field).prop("checked"), tooltip);
                }
                else {
                    chip.create('chips', field.name, field.value, $(field).val(), tooltip);
                }
            }
        });

        chip.addSearchButton();
    }

    static removeFieldValue(fieldId, fieldValue) {
        var field = filter.findElement(fieldId);

        if (field.is('select')) {
            $('#' + field[0].id + ' option[value="' + fieldValue + '"]').prop("selected", false).parent().trigger("change");
        }
        else if (field.is('input[type=checkbox]')) {
            $(field).prop('checked', false);
        }
        else {
            field.val('');
        }
    }

    static findElement(elementId) {
        var field = $('#' + elementId);
        if (!$('#' + elementId).length)
            field = $(document.getElementsByName(elementId)[0]);

        return field;
    }
}

class chip {
    static getTooltip(fieldId) {
        if ($("label[for='" + fieldId + "']").length)
            return $("label[for='" + fieldId + "']")[0].textContent;

        return "";
    }

    static addSearchButton(containerId) {
        if ($('#' + containerId).find('button.chips-search-button').length == 0) {
            var searchButtonTemplate = document.querySelector('#searchButtonTemplate').content.cloneNode(true).firstElementChild;
            if (searchButtonTemplate) {
                $('#' + containerId).append(searchButtonTemplate);
            }
        }
    }

    static create(containerId, chipName, chipValue, chipText, tooltip) {
        var chipTemplate = document.querySelector('#chipTemplate').content.cloneNode(true).firstElementChild;
        $(chipTemplate).attr('data-chipname', chipName);
        $(chipTemplate).attr('data-chipvalue', chipValue);
        $(chipTemplate).attr('data-chiptext', chipText);
        $(chipTemplate).attr('title', tooltip);

        $(chipTemplate).append(chipText.toString());
        $('#' + containerId).append(chipTemplate);
    }

    static removeAll(containerId) {
        $('#' + containerId).empty();
        this.addSearchButton(containerId);
    }

    static remove(chipElement) {
        if (chipElement) {
            $(chipElement).remove();
            filter.removeFieldValue($(chipElement).data('chipname'), $(chipElement).data('chipvalue'));
        }
    }
}



