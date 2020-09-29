/**
 * jQuery Placeholder Plugin 1.0
 *
 * http://johnnyfreeman.github.com/placeholder/
 * Copyright 2011, Johnny Freeman
 * Free to use under the MIT license.
 * http://www.opensource.org/licenses/mit-license.php
 */

;(function($) {

    var placeHolder = function(field, options)
    {
        // save the element to the field 
        // property for future referance
        this.field = $(field);

        // $.extend is a method provided by jQuery that
        // merges the options passed in with the defaults
        $.extend(this.options, options);
        
        // add blur and focus events to the field and 
        // place them in the 'placeHolder' namespace so that
        // if later we want to unbind an event, we only unbind
        // the events bound by this plugin
        this.field.bind({
            'blur.placeHolder': this.restore,
            'focus.placeHolder': this.clear,

            // this step is done by implimenting the Events class in Mootools
            // since jQuery doesn't offer such functionality, we use jQuery's
            // bind method for adding custom events
            'restore.placeHolder': this.options.onRestore,
            'clear.placeHolder': this.options.onClear
        });

        // trigger the restore method immediately so that the value 
        // and placeholder classname will be set on page load.
        // we don't want the onRestore event to be fired a bunch of times on
        // page load so we'll freeze all events until the restore method 
        // has done it's thing
        this.freezeEvents = true;
        this.restore();
        this.freezeEvents = false;

        return this;
    };

    placeHolder.prototype.options =  {
        className: 'placeholder',
        freezeEvents: false,
        onRestore: function(){},
        onClear: function(){}
    }

    placeHolder.prototype.freezeEvents = false; // allows events to be frozen
    placeHolder.prototype.field = null; // holds the field (element)
    placeHolder.prototype.nativeSupport = 'placeholder' in document.createElement('input'); // compatibility flag

    placeHolder.prototype.restore = function(event)
    {
        // only restore the placeholder if the field is blank
        if (!this.field.val().length)
        {
            // add placeholder class
            this.field.addClass(this.options.className);

            // if current browser doesn't support the html5 placeholder 
            // attribute use it's placeholder text as the value
            if (!this.nativeSupport)
            {
                this.field.val(this.field.attr('placeholder'));
            };

            // fire the onRestore event so long
            // as events have not been frozen
            if (!this.freezeEvents)
            {
                this.field.trigger('restore.placeHolder');
            };
        };

        return this;
    };

    placeHolder.prototype.clear = function(event)
    {
        // remove placeholder class
        this.field.removeClass(this.options.className);

        // if this field's value is same as it's placeholder,
        // erase the field's value and fire the onClear event.
        if (this.field.attr('placeholder') == this.field.val())
        {
            // erase the field's value only if the current browser 
            // doesn't support the html5 placeholder attribute
            if (!this.nativeSupport)
            {
                this.field.val('');
            };

            // fire the onClear event so long
            // as events have not been frozen
            if (!this.freezeEvents)
            {
                this.field.trigger('clear.placeHolder');
            };
        };

        return this;
    };

    // create a jquery plugin to instantiate the placeHolder object
    $.fn.placeHolder = function(options) {
        return this.each(function() {
            return new placeHolder(this, options);
        });
    };

})(jQuery);