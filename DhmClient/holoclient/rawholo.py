##########################################################################
# Copyright (c) 2023 Reinhard Caspary                                    #
# <reinhard.caspary@phoenixd.uni-hannover.de>                            #
# This program is free software under the terms of the MIT license.      #
##########################################################################

import numpy as np
from scidatacontainer import Container


class HoloContainer(Container):

    """ SciDataContainer for the storage of digital holograms. """

    containerType = "DigitalHologram"
    containerVersion = 1.0

    def __pre_init__(self):

        """ Initialize all items absorbing the constructor parameters
        holo and params. """

        # Not in creation mode
        if (self.kwargs["file"] is not None) or \
           (self.kwargs["uuid"] is not None):
            return
        
        # Create or update container items
        if self.kwargs["items"] is None:
            items = {}
        else:
            items = self.kwargs["items"]

        # Parameter holo:np.ndarray
        holo = self.kwargs.pop("holo")
        if not isinstance(holo, np.ndarray) or len(holo.shape) != 2:
            raise RuntimeError("Hologram image expected!")

        # Parameter params:dict
        params = self.kwargs.pop("params")
        if not isinstance(params, dict):
            raise RuntimeError("Parameter dictionary expected!")

        # Container type
        content = items.get("content.json", {})
        content["containerType"] = {
            "name": self.containerType,
            "version": self.containerVersion}

        # Basic meta data
        meta = items.get("meta.json", {})
        meta["title"] = "Raw Digital Hologram"
        meta["description"] = "Image from digital holographic microscope."

        # Update items dictionary
        items["content.json"] = content
        items["meta.json"] = meta
        items["meas/image.json"] = params
        items["meas/image.png"] = holo
        self.kwargs["items"] = items


    def __post_init__(self):

        """ Initialize this container. """

        # Type check of the container
        if (self.content["containerType"]["name"] != self.containerType) or \
           (self.content["containerType"]["version"] != self.containerVersion):
            raise RuntimeError("Containertype must be '%s'!" % self.containerType)


    @property
    def img(self):

        """ Shortcut to the hologram image. """
        
        return self["meas/image.png"]


    @property
    def params(self):

        """ Shortcut to the parameter data dictionary. """
        
        return self["meas/image.json"]


##########################################################################
if __name__ == "__main__":

    pass    
