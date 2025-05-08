import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Skeleton } from "@/components/ui/skeleton"
import { cn } from "@/lib/utils"
import { Link } from "@tanstack/react-router"
import { Bed, Building, ChevronDown, ChevronRight, Home, Layers } from "lucide-react"
import { useState } from "react"

import { useBlocks } from "@/lib/hooks/useBlocks"
import { useBuildings } from "@/lib/hooks/useBuildings"
import { useFloors } from "@/lib/hooks/useFloors"
import { useRooms } from "@/lib/hooks/useRooms"

import { BlocksResponse } from "@/lib/types/block"
import { BuildingsResponse } from "@/lib/types/building"
import { FloorsResponse } from "@/lib/types/floor"
import { RoomsResponse, RoomStatus } from "@/lib/types/room"
import { getGenderRuleColor } from "@/lib/utils/genreRuleUtils"

export function BuildingMap() {
  const { data: buildings = [], isLoading, isError } = useBuildings()

  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set())

  const toggle = (id: string) => {
    setExpandedIds(prev => {
      const next = new Set(prev)
      if (next.has(id)) {
        next.delete(id)
      } else {
        next.add(id)
      }
      return next
    })
  }

  const isExpanded = (id: string) => expandedIds.has(id)

  if (isLoading) return <Skeleton className="w-full h-[600px]" />
  if (isError) return <div className="text-destructive p-4">Не вдалося завантажити будівлі</div>

  return (
    <Card>
      <CardHeader>
        <CardTitle>Building Map</CardTitle>
      </CardHeader>
      <CardContent>
        <ScrollArea className="h-[600px] pr-4">
          <div className="space-y-2">
            {buildings.length === 0 ? (
              <div className="p-4 text-center text-muted-foreground">Немає доступних будівель</div>
            ) : (
              buildings.map((building: BuildingsResponse) => (
                <BuildingItem key={building.id} building={building} isExpanded={isExpanded} toggle={toggle} />
              ))
            )}
          </div>
        </ScrollArea>
      </CardContent>
    </Card>
  )
}

interface BuildingItemProps {
  building: BuildingsResponse
  isExpanded: (id: string) => boolean
  toggle: (id: string) => void
}

function BuildingItem({ building, isExpanded, toggle }: BuildingItemProps) {
  const open = isExpanded(building.id)
  const { data: floors = [], isLoading, isError } = useFloors(building.id, open)

  return (
    <div className="border rounded-md overflow-hidden">
      <Button variant="ghost" className="w-full flex items-center justify-between p-3 text-left" onClick={() => toggle(building.id)}>
        <div className="flex items-center">
          <Building className="mr-2 h-5 w-5 text-primary" />
          <span className="font-medium">{building.name}</span>
          {building.address && (
            <span className="ml-2 text-sm text-muted-foreground">({building.address})</span>
          )}
        </div>
        {open ? <ChevronDown className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
      </Button>

      {open && (
        <div className="pl-6 pr-2 pb-2">
          {isLoading ? (
            <Skeleton className="h-10 w-full" />
          ) : isError ? (
            <div className="p-2 text-destructive">Не вдалося завантажити поверхи</div>
          ) : floors.length === 0 ? (
            <div className="p-2 text-muted-foreground">Поверхів не знайдено</div>
          ) : (
            floors.map((floor: FloorsResponse) => (
              <FloorItem key={floor.id} floor={floor} isExpanded={isExpanded} toggle={toggle} />
            ))
          )}
        </div>
      )}
    </div>
  )
}

interface FloorItemProps {
  floor: FloorsResponse
  isExpanded: (id: string) => boolean
  toggle: (id: string) => void
}

function FloorItem({ floor, isExpanded, toggle }: FloorItemProps) {
  const open = isExpanded(floor.id)
  const { data: blocks = [], isLoading, isError } = useBlocks(floor.id, open)

  return (
    <div className="border-l border-l-primary/20 mt-1">
      <Button variant="ghost" className="w-full flex items-center justify-between p-2 text-left" onClick={() => toggle(floor.id)}>
        <div className="flex items-center">
          <Layers className="mr-2 h-5 w-5 text-primary" />
          <span className="font-medium">Floor {floor.number}</span>
          <span className="ml-2 text-sm text-muted-foreground">
            ({floor.blocksCount ?? blocks.length} blocks)
          </span>
        </div>
        {open ? <ChevronDown className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
      </Button>

      {open && (
        <div className="pl-6 pr-2 pb-2">
          {isLoading ? (
            <Skeleton className="h-10 w-full" />
          ) : isError ? (
            <div className="p-2 text-destructive">Не вдалося завантажити блоки</div>
          ) : blocks.length === 0 ? (
            <div className="p-2 text-muted-foreground">Блоків не знайдено</div>
          ) : (
            blocks.map((block: BlocksResponse) => (
              <BlockItem key={block.id} block={block} isExpanded={isExpanded} toggle={toggle} />
            ))
          )}
        </div>
      )}
    </div>
  )
}

interface BlockItemProps {
  block: BlocksResponse
  isExpanded: (id: string) => boolean
  toggle: (id: string) => void
}

function BlockItem({ block, isExpanded, toggle }: BlockItemProps) {
  const open = isExpanded(block.id)
  const { data: rooms = [], isLoading, isError } = useRooms(block.id, open)


  return (
    <div className="border-l border-l-primary/20 mt-1">
      <Button variant="ghost" className="w-full flex items-center justify-between p-2 text-left" onClick={() => toggle(block.id)}>
        <div className="flex items-center">
          <Home className="mr-2 h-5 w-5 text-primary" />
          <span className="font-medium">{block.label}</span>
          <Badge className={cn("ml-2", getGenderRuleColor(block.genderRule))} variant="outline">
            {block.genderRule}
          </Badge>
        </div>
        {open ? <ChevronDown className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
      </Button>

      {open && (
        <div className="pl-6 pr-2 pb-2">
          {isLoading ? (
            <Skeleton className="h-10 w-full" />
          ) : isError ? (
            <div className="p-2 text-destructive">Не вдалося завантажити кімнати</div>
          ) : rooms.length === 0 ? (
            <div className="p-2 text-muted-foreground">Кімнат не знайдено</div>
          ) : (
            rooms.map((room: RoomsResponse) => (
              <RoomItem key={room.id} room={room} />
            ))
          )}
        </div>
      )}
    </div>
  )
}

interface RoomItemProps {
  room: RoomsResponse
}

function RoomItem({ room }: RoomItemProps) {
  const getRoomStatusColor = (status: RoomStatus) => {
    switch (status) {
      case "Available":
        return "bg-green-100 text-green-800"
      case "Occupied":
        return "bg-blue-100 text-blue-800"
      case "Maintenance":
        return "bg-red-100 text-red-800"
      default:
        return "bg-gray-100 text-gray-800"
    }
  }

  return (
    <div className="border-l border-l-primary/20 mt-1">
      <Link to={`/rooms/$roomId`} params={{ roomId: room.id }} className="w-full block">
        <Button variant="ghost" className="w-full flex items-center justify-between p-2 text-left">
          <div className="flex items-center">
            <Bed className="mr-2 h-5 w-5 text-primary" />
            <span className="font-medium">Room {room.label}</span>
            <span className="ml-2 text-sm text-muted-foreground">
              ({room.roomType}, {room.capacity} places)
            </span>
          </div>
          <Badge className={getRoomStatusColor(room.status)} variant="outline">
            {room.status}
          </Badge>
        </Button>
      </Link>
    </div>
  )
}
