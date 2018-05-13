//Declares
var $TMAlert = '#TMAlert',
    $isTooltip = '.isTooltip',
    $tinymce = '.tinymce',
    ModalCreate = '#ModalCreate',
    ModalEdit = '#ModalEdit',
    formCreate = '#FormCreate',
    formEdit = '#FormEdit',
    DateNow = new Date();
var $baseUrl = Host + '/',
    $areaUrl = Host + '/',
    $controllerUrl = $baseUrl + Segment[0] + '/';
var TMLanguages = $.fn.TMLanguage({ host: $baseUrl });
function TMLanguage(lang) {
    lang = lang.toLowerCase().split('.');
    if (lang.length > 1)
        return TMLanguages[lang[0]][lang[1]];
};
function TMLanguageTitle(afterFix) {
    afterFix = afterFix !== undefined ? ' - ' + afterFix : '';
    if (Segment.length == 1) {
        if (TMLanguages.hasOwnProperty(Segment[0].toLowerCase()))
            document.title = TMLanguages[Segment[0].toString().toLowerCase()]['title'] + afterFix;
        else
            if (TMLanguages.hasOwnProperty(Segment[0].toLowerCase()))
                document.title = TMLanguages[Segment[0].toLowerCase()]['title'] + afterFix;
    }
    else if (Segment.length > 1)
        if (TMLanguages.hasOwnProperty(Segment[1].toLowerCase()))
            document.title = TMLanguages[Segment[1].toLowerCase()]['title'] + afterFix;//TMLanguages[Segment[1]]['title'];
};

$(function () {
    TMLanguageTitle('Billing');
    $($isTooltip).tooltip({ animation: false });
});
function removeBackdrop(edit) {
    $(edit).on('hidden.bs.modal', function (e) {
        $('.modal-backdrop').remove();
        $(edit).remove();
    })
};

//Global Function
$('.profile').GetForm({ url: 'Profile/Index' });
$('.changePassword').GetForm({ url: 'Profile/ChangePassword' });
$('.userSetting').GetForm({ url: 'Profile/UserSetting' });
//$('.profile').off('click').on('click', function (e) {
//    getEdit(e, $area + 'Users/PartialProfile', $area + 'api/UsersAPI/Profile', null)
//})
//$('.changePassword').off('click').on('click', function (e) {
//    getEdit(e, $area + 'Users/PartialChangePassword')
//})
//$('.userSetting').off('click').on('click', function (e) {
//    getEdit(e, $area + 'Users/PartialUserSetting')
//})

//TinyMCE
function getTinyMCE() {
    tinymce.init({
        selector: $tinymce,
        mode: 'specific_textareas',//'textareas'
        //theme: 'advanced',
        //force_br_newlines: false,
        //force_p_newlines: false,
        //forced_root_block: '',
        encoding: 'xml',
        entity_encoding: "raw",
        //convert_urls: false,
        theme: 'modern',
        //width: 500,
        //height: 300,
        plugins: [
            'advlist autolink link image lists charmap print preview hr anchor pagebreak spellchecker',
            'searchreplace wordcount visualblocks visualchars code fullscreen insertdatetime media nonbreaking',
            'save table contextmenu directionality emoticons template paste textcolor'
        ],
        //content_css: 'css/content.css',
        toolbar: 'insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image | print preview media fullpage | forecolor backcolor emoticons',
        style_formats: [
            { title: 'Bold text', inline: 'b' },
            { title: 'Red text', inline: 'span', styles: { color: '#ff0000' } },
            { title: 'Red header', block: 'h1', styles: { color: '#ff0000' } },
            { title: 'Example 1', inline: 'span', classes: 'example1' },
            { title: 'Example 2', inline: 'span', classes: 'example2' },
            { title: 'Table styles' },
            { title: 'Table row 1', selector: 'tr', classes: 'tablerow1' }
        ],
        setup: function (editor) {
            editor.on('change', function (i) {
                tinymce.triggerSave();
            });
            editor.on("SaveContent", function (i) {
                i.content = i.content.replace(/&#39/g, "&apos");
            });
        }
    });
}
//FixModal
$(document).on('focusin', function (e) {
    if ($(e.target).closest(".mce-window").length) {
        e.stopImmediatePropagation();
    }
});
//
//$.fn.TMCheckBox('.chkall', '.chkitem', '.btn-chk');