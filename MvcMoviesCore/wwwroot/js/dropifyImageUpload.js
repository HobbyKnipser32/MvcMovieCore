class dropifyImage {

    static Draw(imageAttributes, customMessages) {
        var drEvent = $("#SelectedFile").dropify({
            maxFileSize: '10M',
            allowedFileExtensions: 'xbm tif jfif ico tiff gif svg jpeg svgz jpg webp png bmp pjp apng pjpeg avif',
            messages: customMessages
        });
        drEvent.on('dropify.afterClear', function (e, elem) {
            $('#ImageName').val('');
        });

        if (imageAttributes.imageContent) {
            var b64Data = 'data:image/*;base64,' + imageAttributes.imageContent;
            dropifyImage.SetPreview('SelectedFile', b64Data, imageAttributes.imageName);
        }
    }

    static SetPreview(name, src, fname = '') {
        let input = $('input[name="' + name + '"]');
        let wrapper = input.closest('.dropify-wrapper');
        let preview = wrapper.find('.dropify-preview');
        let filename = wrapper.find('.dropify-filename-inner');
        let render = wrapper.find('.dropify-render').html('');

        input.val('').attr('title', fname);
        wrapper.removeClass('has-error').addClass('has-preview');
        filename.html(fname);

        render.append($('<img />').attr('src', src).css('max-height', input.data('height') || ''));
        preview.fadeIn();
    }
}





