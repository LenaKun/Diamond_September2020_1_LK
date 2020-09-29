
//http://wiki.jqueryui.com/w/page/12138135/Widget%20factory
//http://www.odata.org/developers/protocols/uri-conventions
(function ($) {

    $.widget("demo.grid", {

        self: null,


        // Set up the widget
        _create: function () {
            self = this;
            this._buildTable();
            var data = this.loadData();
            

        },

        // Use the _setOption method to respond to changes to options
        _setOption: function (key, value) {
            switch (key) {
                case "clear":
                    // handle changes to clear option
                    break;
            }
            // In jQuery UI 1.8, you have to manually invoke the _setOption method from the base widget
            $.Widget.prototype._setOption.apply(this, arguments);
            // In jQuery UI 1.9 and above, you use the _super method instead
            this._super("_setOption", key, value);

        },

        // Use the destroy method to clean up any modifications your widget has made to the DOM
        destroy: function () {
            // In jQuery UI 1.8, you must invoke the destroy method from the base widget
            $.Widget.prototype.destroy.call(this);
            // In jQuery UI 1.9 and above, you would define _destroy instead of destroy and not call the base method
        },




        // These options will be used as defaults
        options: {
            //settings
            pagesize: 10,
            baseUrl: '@Url.Content("~/Data/DataService.svc/Histories")',
            currentPage: 1,



            columns: [],
            filter: [],
            skip: function () {
                return (this.currentPage - 1) * self.options.pagesize;
            }
        },

        //functions
        getFieldNames: function () {
            var result = [];
            for (var i in this.options.columns) {
                result.push(this.options.columns[i].name);
            }
            return result;
        },
        getDisplayNames: function () {
            var result = [];
            for (var i in this.options.columns) {
                result.push(this.options.columns[i].displayName);
            }
            return result;
        },

        loadData: function () {
            $.ajax({
                type: "GET",
                url: this._buildUrl(),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                context: this,
                success: function (data, textStatus, jqXHR) {
                    console.log("data:" + data.d[0]);
                    console.log("this:" + this.element.attr('id'));
                    this._renderData(data.d);
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    alert(xhr.status);
                }
            });
        },



        _buildUrl: function () {

            var select = [];

            var expand = this.options.expand ? "&$expand=" + this.options.expand : '';

            for (var index in this.options.columns) {
                select.push(this.options.columns[index].name.replace(".", "/"));
            }
            var select = "&$select=" + select.join(',');

            var skip = "&$skip=" + this.options.skip();
            var top = "&$top=" + this.options.pagesize;
            var filter = "&$filter=";
            var orderby = "&$orderby=" + this.options.orderby;

            return this.options.baseUrl + "?" + expand + select + skip + top + filter + orderby;
        },

        sort: function (sortString) {
            this.options.orderby = sortString.replace('.', '/');
            this.options.currentpage = 1;
            this.loadData();
        },
        setPage: function (sender, page) {
            this.options.page = page;
            this.loadData();
        },
        filter: function (filterString) {

        },

        filter: function () {
        },

        //chreate the sceleton of a table
        _buildTable: function () {
            var head = '<table class="grid"><thead><tr>';
            for (var i = 0; i < this.options.columns.length; i++) {
                head += "<th>";
                head += this._columnHeader(i);
                head += "</th>";
            }
            head += "</tr></thead><tbody></tbody><tfoot></tfoot></table>";

            var existingTables = this.element.find('table.grid');
            if (existingTables.length > 0) {
                existingTables.html($(head).html());
            }
            else {
                this.element.append(head);
            }

            //sort events
            this.element.find('a[data-orderby]').bind("click", { owner: this }, function (event) {
                var owner = event.data.owner;
                console.log('header clicked');
                //todo:check for a valid orderby name
                var orderby = $(this).attr('data-orderby');
                orderby += " ";
                if ($(this).hasClass('asc')) {
                    $(this).removeClass('asc');
                    $(this).addClass('desc');
                    orderby += 'desc';
                }
                else {
                    $(this).removeClass('desc');
                    $(this).addClass('asc');
                    orderby += 'asc';
                }

                owner.sort(orderby);
            });
        },

        //replace the tbody
        _renderData: function (results) {
            var tbody = "";
            for (var itemIndex in results) {

                tbody += '<tr>';
                console.log(itemIndex);
                var row = results[itemIndex];


                for (var field in this.options.columns) {

                    var fName = this.options.columns[field].name;
                    console.log(fName);
                    var splits = fName.split('.');
                    var value = row;
                    for (var fi = 0; fi < splits.length; fi++) {
                        console.log(" " + splits[fi]);

                        console.log(" " + splits[fi] + ' = ' + value[splits[fi]]);
                        value = value[splits[fi]];
                    }


                    if (this.options.columns[field].format) {
                        tbody += '<td>' + this.options.columns[field].format(value) + '</td>';
                    }
                    else {
                        tbody += '<td>' + value + '</td>';
                    }
                }
            }

            this.element.find('table.grid').find('tbody').html(tbody);
        },

        //html for the column header
        _columnHeader: function (colIndex) {
            var column = this.options.columns[colIndex];
            if (column.sortable) {
                var link = ('<a href="#' + column.name + '" data-orderby="' + column.name + '">' + column.displayName + '</a>');

                return link;
            }
            else {
                return column.displayName;
            }
        }


    });

} (jQuery));
