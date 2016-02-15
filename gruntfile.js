module.exports = function (grunt) {
    require('load-grunt-tasks')(grunt);
    var path = require('path');

    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        pkgMeta: grunt.file.readJSON('config/meta.json'),
        dest: grunt.option('target') || 'dist',
        basePath: path.join('<%= dest %>', 'App_Plugins', '<%= pkgMeta.name %>'),




        copy: {
            app_plugins: {
                cwd: 'src/Zippy.Media/App_Plugins/zippyMedia/',
                src: ['**', '!css', '!css/*'],
                dest: '<%= basePath %>',
                expand: true
            },
            dll: {
                cwd: 'src/Zippy.Media/bin/Debug/',
                src: 'Zippy.Media.dll',
                dest: '<%= dest %>/bin/',
                expand: true
            },
            config: {
                cwd: 'src/Zippy.Media/Config/',
                src: 'ZippyMedia.config',
                dest: '<%= dest %>/Config/',
                expand: true
            },

            nuget: {
                files: [
                  {
                      cwd: '<%= dest %>/App_Plugins',
                      src: ['**/*', '!bin', '!bin/*'],
                      dest: 'tmp/nuget/content/App_Plugins',
                      expand: true
                  },
                  {
                      cwd: '<%= dest %>/Config/',
                      src: ['**/*'],
                      dest: 'tmp/nuget/content/Config',
                      expand: true
                  },
                    {
                        cwd: 'src/Zippy.Media/',
                        src: 'web.config.transform',
                        dest: 'tmp/nuget/content',
                        expand: true
                    },
                  {
                      cwd: '<%= dest %>/bin',
                      src: ['*.dll'],
                      dest: 'tmp/nuget/lib',
                      expand: true
                  }
                ]
            },
            
            umbraco: {
                files: [
                 {
                     cwd: '<%= dest %>',
                     src: '**/*',
                     dest: 'tmp/umbraco',
                     expand: true
                 },
                {
                    cwd: 'lib',
                    src: 'PackageActionsContrib.dll',
                    dest: 'tmp/umbraco',
                    expand: true
                }
                ]
            }
        },

        nugetpack: {
            dist: {
                src: 'tmp/nuget/package.nuspec',
                dest: 'pkg'
            }
        },

        template: {
            'nuspec': {
                'options': {
                    'data': {
                        name: '<%= pkgMeta.name %>',
                        version: '<%= pkgMeta.version %>',
                        url: '<%= pkgMeta.url %>',
                        license: '<%= pkgMeta.license %>',
                        licenseUrl: '<%= pkgMeta.licenseUrl %>',
                        author: '<%= pkgMeta.author %>',
                        authorUrl: '<%= pkgMeta.authorUrl %>',
                        files: [{ path: 'tmp/nuget/content/App_Plugins', target: 'content/App_Plugins' }]
                    }
                },
                'files': {
                    'tmp/nuget/package.nuspec': ['config/package.nuspec']
                }
            }
        },

        umbracoPackage: {
            options: {
                name: "<%= pkgMeta.name %>",
                version: '<%= pkgMeta.version %>',
                url: '<%= pkgMeta.url %>',
                license: '<%= pkgMeta.license %>',
                licenseUrl: '<%= pkgMeta.licenseUrl %>',
                author: '<%= pkgMeta.author %>',
                authorUrl: '<%= pkgMeta.authorUrl %>',
                manifest: 'config/package.xml',
                readme: 'config/readme.txt',
                sourceDir: 'tmp/umbraco',
                outputDir: 'pkg',
            }
        },

        clean: {
            build: '<%= grunt.config("basePath").substring(0, 4) == "dist" ? "dist/**/*" : "null" %>',
            tmp: ['tmp']
        },

        assemblyinfo: {
            options: {
                files: ['src/Zippy.Media/Zippy.Media.csproj'],
                filename: 'AssemblyInfo.cs',
                info: {
                    version: '<%= (pkgMeta.version.indexOf("-") ? pkgMeta.version.substring(0, pkgMeta.version.indexOf("-")) : pkgMeta.version) %>',
                    fileVersion: '<%= pkgMeta.version %>'
                }
            }
        },

        msbuild: {
            options: {
                stdout: true,
                verbosity: 'quiet',
                maxCpuCount: 4,
                version: 4.0,
                buildParameters: {
                    WarningLevel: 2,
                    NoWarn: 1607
                }
            },
            dist: {
                src: ['src/Zippy.Media/Zippy.Media.csproj'],
                options: {
                    projectConfiguration: 'Debug',
                    targets: ['Clean', 'Rebuild'],
                }
            }
        }
    });

    grunt.registerTask('default', ['clean', 'assemblyinfo', 'msbuild:dist', 'copy:dll', 'copy:app_plugins', 'copy:config']);
    grunt.registerTask('nuget', ['clean:tmp', 'default', 'copy:nuget', 'template:nuspec', 'nugetpack']);
    grunt.registerTask('umbraco', ['clean:tmp', 'default', 'copy:umbraco', 'umbracoPackage']);
    grunt.registerTask('package', ['clean:tmp', 'default', 'copy:nuget', 'template:nuspec', 'nugetpack', 'copy:umbraco', 'umbracoPackage', 'clean:tmp']);
};