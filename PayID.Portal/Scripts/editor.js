(function ($) {

    var editor = CKEDITOR.instances['Content'];
    if (editor) { editor.destroy(true); }

    CKEDITOR.replace('Content', {
        language: 'vi',
        height: 180,
        width: "100%",
        baseFloatZIndex: 100000,
        toolbar: 'Basic',
    });
}(jQuery));