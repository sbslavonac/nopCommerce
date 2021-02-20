function createCombinationsBehavior(settings) {
  var defaultSettings = {
    contentEl: false,
    fetchUrl: false,
    displayType: 0
  };

  return {
    settings: $.extend({}, defaultSettings, settings),
    params: {
      availableCombinations: [],
      availableAttributeIds: []
    },

    init: function () {
      var $contentEl = $(this.settings.contentEl);
      var $attributes = $('[data-attr]', $contentEl);
      if ($attributes && $attributes.length > 0) {
        var self = this;

        $.each($attributes, function (index, attribute) {
          var id = parseInt($(attribute).data('attr'));
          self.params.availableAttributeIds.push(id);
        });

        $attributes.on('change', function () {
          var id = parseInt($(this).data('attr'));
          self.normalizeSelection(id);
          self.processCombinations();
          self.normalizeSelection(id);
        });
      }

      this.loadCombinations();
    },

    loadCombinations: function () {
      var self = this;

      $.ajax({
        cache: false,
        url: self.settings.fetchUrl,
        type: 'GET',
        success: function (response) {
          self.params.availableCombinations = response;

          var selectedAttributes = self.getSelectedAttributes();
          if (selectedAttributes && selectedAttributes.length > 0) {
            var firstAttribute = selectedAttributes[0];

            self.normalizeSelection(firstAttribute.id);
            self.processCombinations();
            self.normalizeSelection(firstAttribute.id);
          }
        },
        error: function () {
          self.params.availableCombinations = [];
        }
      });
    },

    normalizeSelection: function (selectedAttributeId) {
      if (!selectedAttributeId)
        return;

      var self = this;

      var selectedAttributes = self.getSelectedAttributes();
      if (!selectedAttributes || selectedAttributes.length === 0)
        return;

      var attributeIndex = $.map(selectedAttributes, function (attribute) {
        return attribute.id;
      }).indexOf(selectedAttributeId);

      if (attributeIndex !== -1 && attributeIndex < selectedAttributes.length - 1) {
        var processedAttributes = selectedAttributes.slice(0, attributeIndex + 1);
        var unprocessedAttributes = selectedAttributes.slice(attributeIndex + 1);

        $.each(unprocessedAttributes, function (index, unprocessedAttribute) {
          var combinations = self.getCombinationsByAttributeId(unprocessedAttribute.id, processedAttributes);
          if (combinations && combinations.length > 0) {
            var processedValues = [];

            $.each(combinations, function (index, combination) {
              //var attribute = combination.Attributes.find(function (attribute) {
              //  return attribute.Id === unprocessedAttribute.id;
              //});
              var attribute = $.grep(combination.Attributes, function (attribute, index) {
                return attribute.Id === unprocessedAttribute.id;
              })[0];

              $.each(attribute.ValueIds, function (index, valueId) {
                if (!combination.InStock) {
                  self.unselectValue(valueId);
                }

                if ($.inArray(valueId, processedValues) === -1) {
                  processedValues.push(valueId);
                }
              });
            });

            $.each(unprocessedAttribute.values, function (index, valueId) {
              if ($.inArray(valueId, processedValues) === -1) {
                self.unselectValue(valueId);
              }
            });
          } else {
            $.each(unprocessedAttribute.values, function (index, valueId) {
              self.unselectValue(valueId);
            });
          }

          processedAttributes.push(unprocessedAttribute);
        });
      }
    },

    processCombinations: function () {
      var self = this;

      var availableCombinations = this.params.availableCombinations;
      if (!availableCombinations || availableCombinations.length === 0) {
        // disable/hide all attribute values
        var valueIds = this.getAttributeValueIds();
        $.each(valueIds, function (index, valueId) {
          self.toggleAttributeValue(valueId, false);
        });

        return;
      }

      var selectedAttributes = self.getSelectedAttributes();
      if (selectedAttributes && selectedAttributes.length > 0) {
        var processedAttributes = [];

        $.each(selectedAttributes, function (index, selectedAttribute) {
          var combinations = self.getCombinationsByAttributeId(selectedAttribute.id, processedAttributes);
          if (combinations && combinations.length > 0) {
            var processedValues = [];

            $.each(combinations, function (index, combination) {
              //var attribute = combination.Attributes.find(function (attribute) {
              //  return attribute.Id === selectedAttribute.id;
              //});
              var attribute = $.grep(combination.Attributes, function (attribute, index) {
                return attribute.Id === selectedAttribute.id;
              })[0];

              $.each(attribute.ValueIds, function (index, valueId) {
                //var processedValue = processedValues.find(function (value) {
                //  return value.id === valueId;
                //});
                var processedValue = $.grep(processedValues, function (value) {
                  return value.id === valueId;
                })[0];
                
                if (processedValue) {
                  if (!processedValue.inStock && combination.InStock) {
                    self.toggleAttributeValue(valueId, true);
                    processedValue.inStock = true;
                  }
                } else {
                  self.toggleAttributeValue(valueId, combination.InStock);
                  processedValues.push({ id: valueId, inStock: combination.InStock });
                }
              });
            });

            // toggle unprocessed attribute value
            var valueIds = self.getAttributeValueIds(selectedAttribute.id);
            $.each(valueIds, function (index, valueId) {
              //var processedValue = processedValues.find(function (value) {
              //  return value.id === valueId;
              //});
              var processedValue = $.grep(processedValues, function (value, index) {
                return value.id === valueId;
              })[0];
              if (!processedValue) {
                self.toggleAttributeValue(valueId, false);
              }
            });
          } else {
            // toggle all attribute values
            valueIds = self.getAttributeValueIds(selectedAttribute.id);
            $.each(valueIds, function (index, valueId) {
              self.toggleAttributeValue(valueId, false);
            });
          }

          processedAttributes.push(selectedAttribute);
        });
      }
    },

    getCombinationsByAttributeId: function (attributeId, processedAttributes) {
      var availableCombinations = this.params.availableCombinations;
      if (!availableCombinations || availableCombinations.length === 0) {
        return;
      };

      return $.grep(availableCombinations, function (combination) {
        var found = combination.Attributes.find(function (attribute) {
          return attribute.Id === attributeId;
        });

        if (processedAttributes && processedAttributes.length > 0) {
          $.each(processedAttributes, function (index, processedAttribute) {
            found = found && combination.Attributes.find(function (attribute) {
              var attrbiuteIsFound = attribute.Id === processedAttribute.id;

              // exclude unselected attribute values
              if (processedAttribute.values.length > 0) {
                $.each(processedAttribute.values, function (index, id) {
                  attrbiuteIsFound = attrbiuteIsFound && $.inArray(id, attribute.ValueIds) !== -1
                });
              }

              return attrbiuteIsFound;
            });
          });
        }

        return found;
      });
    },

    getAttributeValueIds: function (attributeId) {
      var $contentEl = $(this.settings.contentEl);
      var $scope = attributeId ? $('[data-attr=' + attributeId + ']', $contentEl) : $contentEl;
      var $valueItems = $('[data-attr-value]', $scope);
      if ($valueItems) {
        return $.map($valueItems, function (item) {
          return parseInt($(item).data('attr-value'));
        });
      }
    },

    toggleAttributeValue: function (valueId, enabled) {
      if (!valueId)
        return;

      var displayType = this.settings.displayType;
      var $contentEl = $(this.settings.contentEl);
      var $value = $('[data-attr-value=' + valueId + ']', $contentEl);
      if (enabled) {
        if (displayType === 0) {
          $value.prop('disabled', false);
          $('input', $value).prop('disabled', false);
          $value.removeClass('disabled');
        } else if (displayType === 1) {
          $value.show();

          var $attr = $value.parent('[data-attr]');
          if (!$attr.is('select')) {
            var visibleValues = $attr.find('[data-attr-value]:visible');
            if (visibleValues && visibleValues.length > 0) {
              $('#product_attribute_label_' + $attr.data('attr')).show();
            }
          } else {
            // it's workaround way, <option/> cannot be checked with ':visible'
            $value.attr('in-stock', 1);

            var visibleOptions = $attr.find('[in-stock=1]');
            if (visibleOptions && visibleOptions.length > 0) {
              $attr.show();
              $('#product_attribute_label_' + $attr.data('attr')).show();
            }
          }
        }
      } else {
        if (displayType === 0) {
          $value.prop('disabled', true);
          $('input', $value).prop('disabled', true);
          $value.addClass('disabled');
        } else if (displayType === 1) {
          $value.hide();

          $attr = $value.parent('[data-attr]');
          if (!$attr.is('select')) {
            visibleValues = $attr.find('[data-attr-value]:visible');
            if (!visibleValues || visibleValues.length === 0) {
              $('#product_attribute_label_' + $attr.data('attr')).hide();
            }
          } else {
            // it's workaround way, <option/> cannot be checked with ':visible'
            $value.attr('in-stock', 0);

            visibleOptions = $attr.find('[in-stock=1]');
            if (!visibleOptions || visibleOptions.length === 0) {
              $attr.hide();
              $('#product_attribute_label_' + $attr.data('attr')).hide();
            }
          }
        }
      }
    },

    unselectValue: function (valueId) {
      if (!valueId)
        return;

      var $contentEl = $(this.settings.contentEl);
      var $value = $('[data-attr-value=' + valueId + ']', $contentEl);
      $value.prop('selected', false);
      $('input', $value).prop('checked', false);
      // for image square
      $value.removeClass('selected-value');
    },

    getSelectedAttributes: function () {
      var availableAttributeIds = this.params.availableAttributeIds;
      if (availableAttributeIds && availableAttributeIds.length > 0) {
        var $contentEl = $(this.settings.contentEl);

        return $.map(availableAttributeIds, function (attributeId) {
          var selectedValues = [];

          var $attribute = $('[data-attr=' + attributeId + ']', $contentEl);
          var $attributeValues = $('[data-attr-value]', $attribute);
          if ($attributeValues && $attributeValues.length > 0) {
            $.each($attributeValues, function (index, value) {
              var $value = $(value);
              if ($value.is(':selected') || $('input', $value).is(':checked')) {
                var id = parseInt($value.data('attr-value'));
                selectedValues.push(id);
              }
            });
          }

          return {
            id: attributeId,
            values: selectedValues
          }
        });
      }
    },
  }
}