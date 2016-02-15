angular.module("umbraco").directive("fileInput", ["$parse", function($parse) {
    return {
        restrict: "A",
        link: function(scope, elm, attrs) {
            elm.bind("change", function() {
                $parse(attrs.fileInput).assign(scope, elm[0].files);
                scope.$apply();
            });
        }
    };
}]);

angular.module('ng').filter('cut', function () {
    return function (value, wordwise, max, tail) {
        if (!value) return '';

        max = parseInt(max, 10);
        if (!max) return value;
        if (value.length <= max) return value;

        value = value.substr(0, max);
        if (wordwise) {
            var lastspace = value.lastIndexOf(' ');
            if (lastspace != -1) {
                value = value.substr(0, lastspace);
            }
        }

        return value + (tail || ' …');
    };
});
