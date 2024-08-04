// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function loadTenantDeviceSelectorPartial() {
    $.ajax({
        url: '/api/Session/Selector',
        type: 'GET',
        success: function (result) {
            $('#tenantDeviceSelectorContainer').html(result);
        },
        error: function (xhr, status, error) {
            $('#tenantDeviceSelectorContainer').html('<p class="text-nexum-light">' + 'Failed to load tenant/device selector.' + '</p>');
            console.error('Error loading partial view:', error);
        }
    });
}