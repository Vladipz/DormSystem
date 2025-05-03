import { BuildingMap } from '@/components/BuildingMap'
import { MaintenanceCenter } from '@/components/MaintenanceCenter'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/_mainLayout/room-dashboard/')({
  component: RouteComponent,
})

function RouteComponent() {
  return <>
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Room Service</h1>
      </div>
      <Tabs defaultValue="building-map" className="w-full">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="building-map">Building Map</TabsTrigger>
          <TabsTrigger value="free-places">Free Places</TabsTrigger>
          <TabsTrigger value="move-wizard">Move In/Out</TabsTrigger>
          <TabsTrigger value="maintenance">Maintenance</TabsTrigger>
        </TabsList>
        <TabsContent value="building-map">
          <BuildingMap></BuildingMap>
        </TabsContent>
        <TabsContent value="free-places">
          <p>HI</p>
        </TabsContent>
        <TabsContent value="move-wizard">
          <p>HI</p>
        </TabsContent>
        <TabsContent value="maintenance">
          <MaintenanceCenter />
        </TabsContent>
      </Tabs>
    </div>
  </>
}
