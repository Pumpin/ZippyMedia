angular.module("umbraco").controller("Zippy.Media.Controller", function ($scope, $http, notificationsService, treeService, navigationService) {

    $scope.uploading = false;
    $scope.files = [];

    $scope.uploadAndUnpack = function(folderId) {

        $scope.uploading = true;
        
        var formData = new FormData();
        angular.forEach($scope.files, function (file) {
            formData.append("file", file);
        });
        
        formData.append("startFolderId", folderId);
        $http.post("/umbraco/backoffice/ZippyMedia/ZippyMedia/UploadFiles", formData,
        {
            transformRequest: angular.identity,
            headers: { "Content-Type": undefined }
        })
        .success(function (response) {
            $scope.uploading = false;
            $scope.files = [];
            
            notificationsService.success("Success", "Your file has been uploaded and unpacked.");
            treeService.loadNodeChildren({ node: $scope.currentNode, section: "media" });
            navigationService.hideNavigation();
        })
        .error(function () {
            $scope.uploading = false;
            $scope.files = [];
            notificationsService.error("Error", "Something went wrong when trying to upload your zip file. check the log for more info.");
        });

    };

    $scope.removeFile = function (index) {
        $scope.files = [];
    };



});




angular.module("umbraco").controller("Zippy.Media.Unpack.Controller", function ($scope, $http, notificationsService, treeService, navigationService) {

    $scope.uploading = false;
    $scope.files = [];

    $http.get("/umbraco/backoffice/ZippyMedia/ZippyMedia/GetFilesFromServer")
      .success(function (response) {
          $scope.files = response.Files;
      })
      .error(function () {
          
      });


    $scope.unpack = function (folderId) {
        $scope.uploading = true;
    };


});

