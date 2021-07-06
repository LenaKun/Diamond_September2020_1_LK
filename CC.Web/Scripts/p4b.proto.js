

//  padLeft
//--------------------------------------------------------------------------------------------------------------
function parseJsonDateString(value) {
	var re = /-?\d+/;
	var m = re.exec(value);
	var d = new Date(parseInt(m[0]));
	return d;
}
//Date.prototype.toString = function (format) {
//	format = format? format:'dd M yy';
//	return $.datepicker.formatDate(format, this);
//}







//  padLeft
//--------------------------------------------------------------------------------------------------------------
String.prototype.padLeft = function (n, pad) {

	var str = pad !== undefined ? String(pad).substring(0, 1) : ' ';
	return Array(n - this.length + 1)
		.join(str)
		+ this;
}
Number.prototype.padLeft = function (n, str) {
	str = str || '0';
	return (this < 0 ? '-' : '') +
			String(Math.abs(this)).padLeft(n, str);
}


//  padLeft
//--------------------------------------------------------------------------------------------------------------
Date.prototype.format = function (f) {
	if (!this.valueOf())
		return ' ';

	var d = this;

	var gsMonthNames = [
			'January',
			'February',
			'March',
			'April',
			'May',
			'June',
			'July',
			'August',
			'September',
			'October',
			'November',
			'December'
	];

	// a global day names array
	var gsDayNames = [
			'Sunday',
			'Monday',
			'Tuesday',
			'Wednesday',
			'Thursday',
			'Friday',
			'Saturday'
	];

	return f.replace(/(yyyy|yy|mmmm|mmm|mm|dddd|ddd|dd|hh|nn|ss|a\/p)/gi,
		function ($1) {
			switch ($1.toLowerCase()) {

				case 'yyyy': return d.getFullYear();
				case 'yy': return String(d.getFullYear()).substr(2, 2);
				case 'mmmm': return gsMonthNames[d.getMonth()];
				case 'mmm': return gsMonthNames[d.getMonth()].substr(0, 3);
				case 'mm': return (d.getMonth() + 1).padLeft(2);
				case 'dddd': return gsDayNames[d.getDay()];
				case 'ddd': return gsDayNames[d.getDay()].substr(0, 3);
				case 'dd': return d.getDate().padLeft(2);
				case 'hh': return ((h = d.getHours() % 12) ? h : 12).padLeft(2);
				case 'nn': return d.getMinutes().padLeft(2);
				case 'ss': return d.getSeconds().padLeft(2);
				case 'a/p': return d.getHours() < 12 ? 'a' : 'p';
			}
		}
	);
}


//  serializ
//--------------------------------------------------------------------------------------------------------------
$.fn.serializeObject = function () {
	var o = {};
	var a = $(this).find(':input').map(function (a, b) { return [b.name, b.value]; });

	$.each(a, function () {
		if (o[this.name] !== undefined) {
			if (!o[this.name].push) {
				o[this.name] = [o[this.name]];
			}
			o[this.name].push(this.value || '');
		} else {
			o[this.name] = this.value || '';
		}
	});
	return o;
};


jQuery(function () {
	jQuery.support.placeholder = false;

});
$(function () {
	test = document.createElement('input');
	support = true;
	if (!support) {
		var active = document.activeElement;
		$(':text').focus(function () {
			if ($(this).attr('placeholder') != '' && $(this).val() == $(this).attr('placeholder')) {
				$(this).val('').removeClass('hasPlaceholder');
			}
		}).blur(function () {
			if ($(this).attr('placeholder') != '' && ($(this).val() == '' || $(this).val() == $(this).attr('placeholder'))) {
				$(this).val($(this).attr('placeholder')).addClass('hasPlaceholder');
			}
		});
		$(':text').blur();
		$(active).focus();
		$('form').submit(function () {
			$(this).find('.hasPlaceholder').each(function () { $(this).val(''); });
		});
	}
});

$.fn.ajaxify = function (target) {
	$(this).submit(function (event) {
		//Prevents other event handlers from being called.
		event.stopImmediatePropagation();

		//Prevents the event from bubbling up the DOM tree, preventing any parent handlers from being notified of the event.
		event.stopPropagation();


		if ($(this).valid()) {
			$.ajax({
				url: this.action,
				type: this.method,
				data: $(this).serialize(),
				success: function (result) {
					$(target).html(result);
					$(target).find('form').ajaxify(target);
				}
			});
		}
		return false;
	});
};
//todo - unfinished
(function ($) {
	$.fn.UpdateChildSelect = function (options) {
		var options = $.extend({
			select: "Id,Name",
			orderby: "Name"

		}, options);


		return this.change(function () {
			var dataUrl = '@Url.Content("~/Data/DataService.svc/")' + options.objectSetName + '?';
			dataUrl += "$select=" + options.select;
			if ($(this).val()) {
				dataUrl += "&$filter=" + options.Filter.replace("{0}", $(this).val());
			}
			dataUrl += "&$orderby=" + options.orderby;
			$.ajax({
				url: dataUrl,
				method: "GET",
				dataType: 'json',
				success: function (data, textStatus, jqXHR) {
					var child = $(options.dependantSelector);
					child.html('');
					$.each(data.d, function (i, value) {
						child.append($('<option/>').attr('value', value.Id).html(value.Name));
					});

				},
				error: function (jqXHR, textStatus, errorThrown) {

				}
			});
		});


		return this;
	};
})(jQuery);


String.prototype.parseDecimal = function () {
	return parseFloat(this.replace(/,/, ''));
};

Number.prototype.formatMoney = function (c, d, t) {
	var n = this,
	c = isNaN(c = Math.abs(c)) ? 2 : c,
	d = d == undefined ? "." : d,
	t = t == undefined ? "," : t, s = n < 0 ? "-" : "",
	i = parseInt(n = Math.abs(+n || 0).toFixed(c)) + "",
	j = (j = i.length) > 3 ? j % 3 : 0;
	return s + (j ? i.substr(0, j) + t : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) + (c ? d + Math.abs(n - i).toFixed(c).slice(2) : "");
};



function ShowForDaysCenter(url, $dc_div, $dcc, agencyId) {
    if (agencyId) {
        $.ajax({
            type: "GET",
            url: url + "/Agencies("+ agencyId +")/AgencyGroup/DayCenter",
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        }).done(function (response) {
            var dc = response.d.DayCenter;
            if (dc == true) {
               
                $dc_div.show();
            }
            else
            {
            	if ($dcc) {
            		$dcc.val('');
            	}
                $dc_div.hide();
                }
            }          
        ).fail(function () {

        });
    }
    else {
        $dc_div.hide();
    }

}


function ShowForSupportiveCommunity(url, $sc_div, $sc, agencyId) {
    if (agencyId) {
        $.ajax({
            type: "GET",
            url: url + "/Agencies(" + agencyId + ")/AgencyGroup/SupportiveCommunities",
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        }).done(function (response) {
            var sc = response.d.SupportiveCommunities;
            if (sc == true) {

                $sc_div.show();
            }
            else {
            	if ($sc) {
            		$sc.val('');
            	}
                $sc_div.hide();
            } 
        }
        ).fail(function () {

        });
    }
    else {
        $sc_div.hide();
    }
}

function UpdateCountryAndStates(url, $states, countryId, emptyItemText) {
    if (countryId) {

        $states.html("<option>Working...</option>");
        $states.attr('disabled', 'disabled');
        $.ajax({
            type: "GET",
            url: url + "Countries(" + countryId + ")/States",
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        }).done(function (response) {
            var states = response.d;
            if (states.length > 0 ) {
                $states.html('');
                $states.html('<option value="">' + emptyItemText + '</option>');
                $states.removeAttr('disabled');
                for (var i in states) {
                    var item = states[i];
                    var option = $("<option></option>").attr("value", item.Id).text(item.Name);
                    $states.append(option);
                }
            }
            //else if (states.length > 0 && stateId != "")
            //{
            //    $states.html('');
            //    $states.removeAttr('disabled');
            //    for (var i in states)
            //    {
            //        var item = states[i];
            //        if (item.Id == stateId) {

            //            $states.html('<option value="">' + states[i].item.Name + '</option>');
            //        }
            //            var option = $("<option></option>").attr("value", item.Id).text(item.Name);
            //            $states.append(option);
            //         }
            //    }
                
           
                   
            else {
                $states.html('<option value="">N/A</option>');
            }
        }).fail(function () {

        });
    }
    else {
        $states.attr('disabled', 'disabled');
        $states.html('<option value="">No options available</option>');
    }

}


Number.prototype.toMoney = function (decimals, decimal_sep, thousands_sep) {


	var c = isNaN(decimals) ? 2 : Math.abs(decimals); //if decimal is zero we must take it, it means user does not want to show any decimal
	if (c > 4) { c = 4 }
	var d = decimal_sep || '.'; //if no decimal separator is passed we use the dot as default decimal separator (we MUST use a decimal separator)

	/*
	according to [http://stackoverflow.com/questions/411352/how-best-to-determine-if-an-argument-is-not-sent-to-the-javascript-function]
	the fastest way to check for not defined parameter is to use typeof value === 'undefined' 
	rather than doing value === undefined.
	*/
	var t = (typeof thousands_sep === 'undefined') ? ',' : thousands_sep; //if you don't want to use a thousands separator you can pass empty string as thousands_sep value
    //round the number closes sugnificant digit	
	var n = n = Math.round(this * Math.pow(10, c)) / Math.pow(10, c);
	//extracting the absolute value of the integer part of the number and converting to string
	var i = parseInt(n = Math.abs(n).toFixed(c)) + '';

	var sign = (this < 0) ? '-' : '';
    
	var j = ((j = i.length) > 3) ? j % 3 : 0;

	return sign + (j ? i.substr(0, j) + t : '') + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) + (c ? d + Math.abs(n - i).toFixed(c).slice(2) : '');
}


parseDate = function (val) {
	var a = val.split(/[^0-9]/);
	var now = new Date(a[0], a[1] - 1, a[2], a[3], a[4], a[5]);
	return now;
}
renderDate = function (val, format, fixEnd) {

	if (val) {
		var now = parseDate(val);
		if (format == null || format == undefined) format = 'dd M yy';
		if (Object.prototype.toString.call(now) === "[object Date]") {

			if (!isNaN(fixEnd) && now.getDate) {
				now.setDate(now.getDate() - 1);
			}
			var s = $.datepicker.formatDate(format, now);
			return s;
		}
		else {
			return val;
		}
	} else {
		return val;
	}
};
renderDateTime = function (val) {
	if (val == null) {
		return null;
	}
	var d = parseDate(val);
	if (d.getDate) {
		return $.datepicker.formatDate('dd M yy', d) + " "
			+ d.getHours().padLeft(2, '0') + ":"
		+ d.getMinutes().padLeft(2, '0') + ":"
		+ d.getSeconds().padLeft(2, '0');
	}
	return null;
}
renderDecimal = function (val, decimals) {
	if (decimals == null) {
		decimals = decimalDisplayDigits;
	}

	var result = "N/A";

	if (val != null) {
	    var num = new Number(val)
		result = num.toMoney(decimals);
	}
	return result;
};
renderMrPeriod = function (a, b) {
	if (val) {
		if (Object.prototype.toString.call(now) === "[object Date]") {
			var s = $.datepicker.formatDate('dd M yy', now);
			return s;
		}
		else {
			return val;
		}
	} else {
		return val;
	}
};

parseISO8601String = function (dateString) {
	var timebits = /^([0-9]{4})-([0-9]{2})-([0-9]{2})T([0-9]{2}):([0-9]{2})(?::([0-9]*)(\.[0-9]*)?)?(?:([+-])([0-9]{2})([0-9]{2}))?/;
	var m = timebits.exec(dateString);
	var resultDate;
	if (m) {
		var utcdate = Date.UTC(parseInt(m[1]),
                               parseInt(m[2]) - 1, // months are zero-offset (!)
                               parseInt(m[3]),
                               parseInt(m[4]), parseInt(m[5]), // hh:mm
                               (m[6] && parseInt(m[6]) || 0),  // optional seconds
                               (m[7] && parseFloat(m[7]) * 1000) || 0); // optional fraction
		// utcdate is milliseconds since the epoch
		if (m[9] && m[10]) {
			var offsetMinutes = parseInt(m[9]) * 60 + parseInt(m[10]);
			utcdate += (m[8] === '+' ? -1 : +1) * offsetMinutes * 60000;
		}
		resultDate = new Date(utcdate);
	} else {
		resultDate = null;
	}
	return resultDate;
}