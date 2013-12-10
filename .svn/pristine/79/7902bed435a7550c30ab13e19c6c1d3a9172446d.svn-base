/* 
    Created on  :   Nov 10, 2013, 1:22:31 PM
    Author      :   James Mapa
    Parameters  :   title - Title of the Dialog, 
                    context - Message context to display in Dialog,
                    subcontext - Message sub-context to display in Dialog,
                    type - Type of Pop-up Dialog (exclamation, information, question)
                    transition - Type of Transition for displaying (none, pop, fade, flip, turn, flow, slide, slidefade, slideup, slidedown)
    How to use  :
        1.  Call the function insertDialogs(this) in $(<page id>).on("pagebeforeshow", function(event, ui).
        2.  To use Pop-up Dialogs accordingly, assign the desired argument value to the parameter "type".
            Ex. a. Pop-up Information Dialog
                   commonDialog('New Customer','Customer was added successfully.',
                         'Click Ok to proceed on a new Customer entry form.',
                         'information','pop');
                b. Pop-up Exclamation Dialog
                   commonDialog('Save Customer','There are some blank fields in the entry.',
                         'Important fields are Customer Name, Address1, Zip Code, Mobile Phone and Email Address.',
                         'exclamation','pop');
                c. Pop-up Question Dialog
                   commonDialog('Delete Customer','Are you sure to delete this Customer?',
                         '','question','pop');
        3.  For handling the <Yes> button click in Pop-up Question Dialog, 
            follow the function name format: <page id>_popQues_btnYes_click() and add your codes to correspond the action.
            Ex. function pg_customers_popQues_btnYes_click(){ <your code here...>; }
*/

function insertDialogs(parent){    
    l_parentid = parent.id;
    if ($('#'+l_parentid+'_popup_Information').length === 0){
        injectPage('pg_commondialog.html',l_parentid);    
        rebuild_elementid();    
        $("#"+l_parentid).page('destroy').page();
    }    
}
function rebuild_elementid(){
    //for popup Information elements
    $("#popup_Information").attr("id",l_parentid+"_popup_Information");
    $('#popInfo_h1Title').attr("id",l_parentid+"_popInfo_h1Title");
    $('#popInfo_h3Context').attr("id",l_parentid+"_popInfo_h3Context");
    $('#popInfo_pSubContext').attr("id",l_parentid+"_popInfo_pSubContext");
    
    //for popup Exclamation elements
    $("#popup_Exclamation").attr("id",l_parentid+"_popup_Exclamation");
    $('#popExlc_h1Title').attr("id",l_parentid+"_popExlc_h1Title");
    $('#popExlc_h3Context').attr("id",l_parentid+"_popExlc_h3Context");
    $('#popExlc_pSubContext').attr("id",l_parentid+"_popExlc_pSubContext");     
    
    //for popup Question elements
    $("#popup_Question").attr("id",l_parentid+"_popup_Question");
    $('#popQues_h1Title').attr("id",l_parentid+"_popQues_h1Title");
    $('#popQues_h3Context').attr("id",l_parentid+"_popQues_h3Context");
    $('#popQues_pSubContext').attr("id",l_parentid+"_popQues_pSubContext");
    //$('#popQues_btnYes').attr("href","#"+l_parentid);
}
function commonDialog (title,context,subcontext,type,transition) {    
    switch (type){
        case 'information':            
            setTimeout(function(){show_popInformation(title,context,subcontext,transition);},500);
            break;
        case 'exclamation':
            setTimeout(function(){show_popExclamation(title,context,subcontext,transition);},500);
            break;
        case 'question':
            var l_ans;            
            l_ans = show_popQuestion(title,context,subcontext,transition);
            break;
        default:
    }
}
function show_popInformation(title,context,subcontext,transition){
    $('#'+l_parentid+'_popInfo_h1Title').text(title);
    $('#'+l_parentid+'_popInfo_h3Context').text(context);
    $('#'+l_parentid+'_popInfo_pSubContext').text(subcontext);
    $('#'+l_parentid+'_popup_Information').popup('open', {transition: transition});
}
function show_popExclamation(title,context,subcontext,transition){
    $('#'+l_parentid+'_popExlc_h1Title').text(title);
    $('#'+l_parentid+'_popExlc_h3Context').text(context);
    $('#'+l_parentid+'_popExlc_pSubContext').text(subcontext);
    $('#'+l_parentid+'_popup_Exclamation').popup('open', {transition: transition});
}
function show_popQuestion(title,context,subcontext,transition){
    $('#'+l_parentid+'_popQues_h1Title').text(title);
    $('#'+l_parentid+'_popQues_h3Context').text(context);
    $('#'+l_parentid+'_popQues_pSubContext').text(subcontext);
    $('#'+l_parentid+'_popup_Question').popup('open', {transition: transition});
}
function popQues_btnYes_click(){    
    var callback_function = new Function(l_parentid+'_popQues_btnYes_click()');
    callback_function();
}