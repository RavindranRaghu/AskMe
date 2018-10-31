
$(function () {

    $(document).on('click', '.openExistingSubFeature', function () {
        $('#saveProject').html('Update');
        $('#saveProject').removeAttr('disabled');
        $('#feature-id').val($(this).data('featureid'));
        $('#subfeature-id').val($(this).data('subfeatureid'));
        $('#subfeature-name').val($(this).data('subfeaturename'));
        $('#subfeature-desc').val($(this).data('subfeaturedesc'));
        $('#subfeature-story').val($(this).data('subfeaturestory'));
        console.log($('#subfeature-story').val());
        if ($('#subfeature-story').val() == "1") {
            $('#subfeature-story option:selected').text("Yes");
        }
        else {
            $('#subfeature-story option:selected').text("No");
        }
        $('#subfeature-dev').val($(this).data('subfeaturedev'));
        if ($('#subfeature-dev').val() == "1") {
            $('#subfeature-dev option:selected').text("Yes");
        }
        else {
            $('#subfeature-dev option:selected').text("No");
        }
        $('#subestimated').val($(this).data('estimated'));
        $('#subactual').val($(this).data('actual'));
        $('#lastUpdated').html('Last Updated: ' + $(this).data('lastupdated'));
        $('#story-name').html("Story - " + $(this).data('featurename'));
        $('#operation').val("u");
    });

    //open Modal for delete

    $(document).on('click', '.deleteExistingSubFeature', function () {
        $('#saveProject').html('Delete');
        $('#saveProject').removeAttr('disabled');
        $('#feature-id').val($(this).data('featureid'));
        $('#subfeature-id').val($(this).data('subfeatureid'));
        $('#subfeature-name').val($(this).data('subfeaturename'));
        $('#subfeature-desc').val($(this).data('subfeaturedesc'));
        $('#subfeature-story').val($(this).data('subfeaturestory'));
        console.log($('#subfeature-story').val());
        if ($('#subfeature-story').val() == "1") {
            $('#subfeature-story option:selected').text("Yes");
        }
        else {
            $('#subfeature-story option:selected').text("No");
        }
        $('#subfeature-dev').val($(this).data('subfeaturedev'));
        if ($('#subfeature-dev').val() == "1") {
            $('#subfeature-dev option:selected').text("Yes");
        }
        else {
            $('#subfeature-dev option:selected').text("No");
        }
        $('#subestimated').val($(this).data('estimated'));
        $('#subactual').val($(this).data('actual'));
        $('#lastUpdated').html('Last Updated: ' + $(this).data('lastupdated'));
        $('#story-name').html("Story - " + $(this).data('featurename'));
        $('#operation').val("d");
    });


    //open Modal for new project

    $(document).on('click', '.newSubFeature', function () {
        $('#saveProject').html('Add');
        $('#saveProject').removeAttr('disabled');
        $('#feature-id').val($(this).data('featureid'));
        $('#subfeature-id').val(0);
        $('#lastUpdated').html('Adding New Feature');
        $('#story-name').html("Story - " + $(this).data('featurename'));
        $('#operation').val("a");
    });

    // save project

    $(document).on('click', '#saveSubFeature', function () {
        var featureId = $('#feature-id').val(); 
        var subfeatureId = $('#subfeature-id').val(); 
        var subfeatureName = $('#subfeature-name').val();
        var subfeatureDesc = $('#subfeature-desc').val();
        var subfeatureStory = ($('#subfeature-story').val() == 1) ? true : false;
        var subfeatureDev = ($('#subfeature-dev').val() == 1) ? true : false;
        var subEstimated = $('#subestimated').val();
        var subActual = $('#subactual').val();
        var operation = $('#operation').val();
        var updatedDate = new Date;
        var project = { SubFeatureId: subfeatureId, SubFeatureName: subfeatureName, SubFeatureDesc: subfeatureDesc, StoryGroomed: subfeatureStory, DevelopmentComplete: subfeatureDev, FeatureId: featureId, EffortEstimated: subEstimated, EffortActual: subActual, UpdatedDate: updatedDate };
        var finalProject = { subfeature: project, operation: operation };
        $.ajax({
            type: "POST",
            url: "/admin/SubFeatureUpdate/",
            data: JSON.stringify(finalProject),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (result) {
                if (result == true) {
                    $('#lastUpdatedSub').html('Updated Successfully <span style="color:green" class="glyphicon glyphicon-ok-circle"> </span>');
                    $('#table-reload').val("y");
                    $('#saveSubFeature').attr('disabled', 'disabled');
                }
                else {
                    $('#lastUpdatedSub').html('Updated Failed <span style="color:red" class="glyphicon glyphicon-remove-circle"> </span>');
                    $('#table-reload').val("n");
                    $('#saveSubFeature').removeAttr('disabled');
                }
            },
            error: function (errorMsg) {
                $('#lastUpdatedSub').html('<span class="alert alert-warning">' + errorMsg.statusText + '</span> ');
            }
        });
    });



    //Close Modal

    $(document).on('click', '#closeSubFeature', function () {
        if ($('#table-reload').val() == "y") {
            location.reload();
        }
    });

    $('#parent').change(function () {
        $('#parent-id').val($(this).val());
    });

});

