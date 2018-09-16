
$(function () {

    $(document).on('click', '.openExistingProject', function () {
        $('#saveProject').html('Update');
        $('#saveProject').removeAttr('disabled');
        $('#feature-id').val($(this).data('featureid'));
        $('#feature-name').val($(this).data('featurename'));
        $('#feature-desc').val($(this).data('featuredesc'));
        $('#feature-story').val($(this).data('featurestory'));
        console.log($('#feature-story').val());
        if ($('#feature-story').val() == "1") {
            $('#feature-story option:selected').text("Yes");
        }
        else {
            $('#feature-story option:selected').text("No");
        }
        $('#feature-dev').val($(this).data('featuredev'));
        if ($('#feature-dev').val() == "1") {
            $('#feature-dev option:selected').text("Yes");
        }
        else {
            $('#feature-dev option:selected').text("No");
        }
        $('#lastUpdated').html('Last Updated: ' + $(this).data('lastupdated'));
        $('#operation').val("u");
    });

    //open Modal for delete

    $(document).on('click', '.deleteExistingProject', function () {
        $('#saveProject').html('Delete');
        $('#saveProject').removeAttr('disabled');
        $('#feature-id').val($(this).data('featureid'));
        $('#feature-name').val($(this).data('featurename'));
        $('#feature-desc').val($(this).data('featuredesc'));
        $('#feature-story').val($(this).data('featurestory'));
        if ($('#feature-story').val() == "1") {
            $('#feature-story option:selected').text("Yes");
        }
        else {
            $('#feature-story option:selected').text("No");
        }
        $('#feature-dev').val($(this).data('featuredesc'));
        if ($('#feature-dev').val() == "1") {
            $('#feature-dev option:selected').text("Yes");
        }
        else {
            $('#feature-dev option:selected').text("No");
        }
        $('#lastUpdated').html('Last Updated: ' + $(this).data('lastupdated'));
        $('#operation').val("d");
    });


    //open Modal for new project

    $(document).on('click', '.newProject', function () {
        $('#saveProject').html('Add');
        $('#saveProject').removeAttr('disabled');
        $('#feature-id').val(0);
        $('#lastUpdated').html('Adding New Feature');
        $('#operation').val("a");
    });

    // save project

    $(document).on('click', '#saveProject', function () {
        var featureId = $('#feature-id').val(); // $('#intendId').val();
        var featureName = $('#feature-name').val();
        var featureDesc = $('#feature-desc').val();
        var featureStory = ($('#feature-story').val() == 1) ? true : false;
        var featureDev = ($('#feature-dev').val() == 1) ? true : false;
        var operation = $('#operation').val();
        var updatedDate = new Date;
        var project = { FeatureId: featureId, FeatureName: featureName, FeatureDesc: featureDesc, StoryGroomed: featureStory, DevelopmentComplete: featureDev, UpdatedDate: updatedDate };
        var finalProject = { feature: project, operation: operation };
        $.ajax({
            type: "POST",
            url: "/home/FeatureUpdate/",
            data: JSON.stringify(finalProject),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (result) {
                if (result == true) {
                    $('#lastUpdated').html('Updated Successfully <span style="color:green" class="glyphicon glyphicon-ok-circle"> </span>');
                    $('#table-reload').val("y");
                    $('#saveProject').attr('disabled', 'disabled');
                }
                else {
                    $('#lastUpdated').html('Updated Failed <span style="color:red" class="glyphicon glyphicon-remove-circle"> </span>');
                    $('#table-reload').val("n");
                    $('#saveProject').removeAttr('disabled');
                }
            },
            error: function (errorMsg) {
                $('#lastUpdated').html('<span class="alert alert-warning">' + errorMsg.statusText + '</span> ');
            }
        });
    });



    //Close Modal

    $(document).on('click', '#closeProject', function () {
        if ($('#table-reload').val() == "y") {
            location.reload();
        }
    });

    $('#parent').change(function () {
        $('#parent-id').val($(this).val());
    });

});

